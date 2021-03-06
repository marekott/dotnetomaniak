namespace Kigg.Web
{
    using System;
    using System.Web.Mvc;

    using DomainObjects;
    using Infrastructure;
    using Repository;
    using Service;

    [Compress]
    public class CommentController : BaseController
    {
        private readonly IStoryRepository _storyRepository;
        private readonly IStoryService _storyService;

        public CommentController(IStoryRepository storyRepository, IStoryService storyService)
        {
            Check.Argument.IsNotNull(storyRepository, "storyRepository");
            Check.Argument.IsNotNull(storyService, "storyService");

            _storyRepository = storyRepository;
            _storyService = storyService;
        }

        public reCAPTCHAValidator CaptchaValidator
        {
            get;
            set;
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Post(string id, string body, bool? subscribe)
        {
            id = id.NullSafe();
            body = body.NullSafe();

            string captchaChallenge = null;
            string captchaResponse = null;
            bool captchaEnabled = !CurrentUser.ShouldHideCaptcha();

            if (captchaEnabled)
            {
                captchaChallenge = HttpContext.Request.Form[CaptchaValidator.ChallengeInputName];
                captchaResponse = HttpContext.Request.Form[CaptchaValidator.ResponseInputName];
            }

            JsonViewData viewData = Validate<JsonViewData>(
                                                            new Validation(() => string.IsNullOrEmpty(id), "Identyfikator artyku�u nie mo�e by� pusty."),
                                                            new Validation(() => id.ToGuid().IsEmpty(), "Niepoprawny identyfikator artyku�u."),
                                                            new Validation(() => string.IsNullOrEmpty(body.NullSafe()), "Komentarz nie mo�e by� pusty."),
                                                            new Validation(() => captchaEnabled && string.IsNullOrEmpty(captchaChallenge), "Pole Captcha nie mo�e by� puste."),
                                                            new Validation(() => captchaEnabled && string.IsNullOrEmpty(captchaResponse), "Pole Captcha nie mo�e by� puste."),
                                                            new Validation(() => !IsCurrentUserAuthenticated, "Nie jeste� zalogowany."),
                                                            new Validation(() => captchaEnabled && !CaptchaValidator.Validate(CurrentUserIPAddress, captchaChallenge, captchaResponse), "Weryfikacja Captcha nie powiod�a si�")
                                                          );

            if (viewData == null)
            {
                try
                {
                    using (IUnitOfWork unitOfWork = UnitOfWork.Get())
                    {
                        IStory story = _storyRepository.FindById(id.ToGuid());

                        if (story == null)
                        {
                            viewData = new JsonViewData { errorMessage = "Podany artyku� nie istnieje." };
                        }
                        else
                        {
                            CommentCreateResult result = _storyService.Comment(
                                                                                story,
                                                                                string.Concat(Settings.RootUrl, Url.RouteUrl("Detail", new { name = story.UniqueName })),
                                                                                CurrentUser,
                                                                                body,
                                                                                subscribe ?? false,
                                                                                CurrentUserIPAddress,
                                                                                HttpContext.Request.UserAgent,
                                                                                ((HttpContext.Request.UrlReferrer != null) ? HttpContext.Request.UrlReferrer.ToString() : null),
                                                                                HttpContext.Request.ServerVariables
                                                                             );

                            viewData = string.IsNullOrEmpty(result.ErrorMessage) ? new JsonCreateViewData { isSuccessful = true } : new JsonViewData { errorMessage = result.ErrorMessage };

                            unitOfWork.Commit();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);

                    viewData = new JsonViewData { errorMessage = FormatStrings.UnknownError.FormatWith("dodawania komentarza.") };
                }
            }

            return Json(viewData);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ConfirmSpam(string storyId, string commentId)
        {
            JsonViewData viewData = Validate<JsonViewData>(
                                                            new Validation(() => string.IsNullOrEmpty(storyId), "Identyfikator artyku�u nie mo�e by� pusty."),
                                                            new Validation(() => storyId.ToGuid().IsEmpty(), "Niepoprawny identyfikator artyku�u."),
                                                            new Validation(() => string.IsNullOrEmpty(commentId), "Identyfikator komentarza nie mo�e by� pusty."),
                                                            new Validation(() => commentId.ToGuid().IsEmpty(), "Niepoprawny identyfikator komentarza."),
                                                            new Validation(() => !IsCurrentUserAuthenticated, "Nie jeste� zalogowany."),
                                                            new Validation(() => !CurrentUser.CanModerate(), "Nie masz praw do wo�ania tej metody.")
                                                          );

            if (viewData == null)
            {
                try
                {
                    using (IUnitOfWork unitOfWork = UnitOfWork.Get())
                    {
                        IStory story = _storyRepository.FindById(storyId.ToGuid());

                        if (story == null)
                        {
                            viewData = new JsonViewData { errorMessage = "Poday artyku� nie istnieje." };
                        }
                        else
                        {
                            IComment comment = story.FindComment(commentId.ToGuid());

                            if (comment == null)
                            {
                                viewData = new JsonViewData { errorMessage = "Podany komentarz nie istnieje." };
                            }
                            else
                            {
                                _storyService.Spam(comment, string.Concat(Settings.RootUrl, Url.RouteUrl("Detail", new { name = story.UniqueName })), CurrentUser);

                                unitOfWork.Commit();

                                viewData = new JsonViewData { isSuccessful = true };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);

                    viewData = new JsonViewData { errorMessage = FormatStrings.UnknownError.FormatWith("zaznaczania komentarza jako spam") };
                }
            }

            return Json(viewData);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult MarkAsOffended(string storyId, string commentId)
        {
            JsonViewData viewData = Validate<JsonViewData>(
                                                            new Validation(() => string.IsNullOrEmpty(storyId), "Identyfikator artyku�u nie mo�e by� pusty."),
                                                            new Validation(() => storyId.ToGuid().IsEmpty(), "Niepoprawny identyfikator artyku�u."),
                                                            new Validation(() => string.IsNullOrEmpty(commentId), "Identyfikator komentarza nie mo�e by� pusty."),
                                                            new Validation(() => commentId.ToGuid().IsEmpty(), "Niepoprawny identyfikator komentarza."),
                                                            new Validation(() => !IsCurrentUserAuthenticated, "Nie jeste� zalogowany."),
                                                            new Validation(() => !CurrentUser.CanModerate(), "Nie masz praw do wo�ania tej metody.")
                                                          );

            if (viewData == null)
            {
                try
                {
                    using (IUnitOfWork unitOfWork = UnitOfWork.Get())
                    {
                        IStory story = _storyRepository.FindById(storyId.ToGuid());

                        if (story == null)
                        {
                            viewData = new JsonViewData { errorMessage = "Podany artyku� nie istnieje." };
                        }
                        else
                        {
                            IComment comment = story.FindComment(commentId.ToGuid());

                            if (comment == null)
                            {
                                viewData = new JsonViewData { errorMessage = "Podany komentarz nie istnieje." };
                            }
                            else
                            {
                                _storyService.MarkAsOffended(comment, string.Concat(Settings.RootUrl, Url.RouteUrl("Detail", new { name = story.UniqueName })), CurrentUser);

                                unitOfWork.Commit();

                                viewData = new JsonViewData { isSuccessful = true };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);

                    viewData = new JsonViewData { errorMessage = FormatStrings.UnknownError.FormatWith("zaznaczania komentarza jako obra�liwy") };
                }
            }

            return Json(viewData);
        }
    }
}
