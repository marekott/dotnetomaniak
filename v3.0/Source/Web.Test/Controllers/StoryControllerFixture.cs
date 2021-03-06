﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Moq;
using Xunit;

namespace Kigg.Web.Test
{
    using DomainObjects;
    using Infrastructure;
    using Repository;
    using Service;

    using Kigg.Test.Infrastructure;

    public class StoryControllerFixture : BaseFixture
    {
        private const string UserName = "dummyUser";
        private const string CategoryName = "ASP.NET";
        private const string TagName = "ASPNETMVC";

        private readonly Mock<ICategoryRepository> _categoryRepository;
        private readonly Mock<ITagRepository> _tagRepository;
        private readonly Mock<IStoryRepository> _storyRepository;
        private readonly Mock<IUserRepository> _userRepository;

        private readonly Mock<IStoryService> _storyService;
        private readonly Mock<IContentService> _contentService;

        private readonly HttpContextMock _httpContext;
        private readonly StoryController _controller;

        public StoryControllerFixture()
        {
            RouteTable.Routes.Clear();
            new RegisterRoutes(settings.Object).Execute();

            _categoryRepository = new Mock<ICategoryRepository>();
            _tagRepository = new Mock<ITagRepository>();
            _storyRepository = new Mock<IStoryRepository>();
            _userRepository = new Mock<IUserRepository>();
            _storyService = new Mock<IStoryService>();

            _contentService = new Mock<IContentService>();

            _controller = new StoryController(_categoryRepository.Object, _tagRepository.Object, _storyRepository.Object, _storyService.Object, _contentService.Object, new ISocialServiceRedirector[] { new FaceBookRedirector(), new DeliciousRedirector() })
                              {
                                  UserRepository = _userRepository.Object,
                                  Settings = settings.Object,
                                  CaptchaValidator = null,
                                  StoryRepository = _storyRepository.Object
                              };
            _storyRepository.Setup(x => x.CountByUpcoming()).Returns(0);
            _httpContext = _controller.MockHttpContext("/Kigg", null, null);
            _httpContext.HttpRequest.SetupGet(r => r.UserHostAddress).Returns("192.168.0.1");
        }

        [Fact]
        public void Published_Should_Render_List_View()
        {
            var result = Published();
            var viewData = (StoryListViewData) (result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Najnowsze artykuły o .NET".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Wszystkie", viewData.Subtitle);
            Assert.Equal("Brak opublikowanych artykułów.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Published_Should_Use_StoryRepository()
        {
            Published();

            _storyRepository.Verify();
        }

        [Fact]
        public void GetPublished_Should_Return_Json_Result()
        {
            var pagedResult = GetPublished();

            Assert.Equal(2, pagedResult.Result.Count);
            Assert.Equal(10, pagedResult.Total);
        }

        [Fact]
        public void GetPublished_Should_Use_StoryRepository()
        {
            GetPublished();

            _storyRepository.Verify();
        }

        [Fact]
        public void GetPublished_Should_Log_Exception()
        {
            GetPublishedOnException();

            log.Verify();
        }

        [Fact]
        public void GetPublished_Should_Return_Empty_Result_When_Exception_Occurrs()
        {
            var pagedResult = GetPublishedOnException();

            Assert.Empty(pagedResult.Result);
            Assert.Equal(0, pagedResult.Total);
        }

        [Fact]
        public void Category_Should_Render_List_View()
        {
            var result = Category();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Najnowsze artykuły o .NET w dziale {1}".FormatWith(settings.Object.SiteTitle, CategoryName), viewData.Title);
            Assert.Equal(CategoryName, viewData.Subtitle);
            Assert.Equal("Brak opublikowanych artykułów w \"{0}\".".FormatWith(CategoryName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void SecondPage_Category_Should_Render_List_View()
        {
            var result = Category(2);
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Najnowsze artykuły o .NET w dziale {1} - Strona 2".FormatWith(settings.Object.SiteTitle, CategoryName), viewData.Title);
            Assert.Equal(CategoryName, viewData.Subtitle);
            Assert.Equal("Brak opublikowanych artykułów w \"{0}\".".FormatWith(CategoryName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Category_Should_Use_CategoryRepository()
        {
            Category();

            _categoryRepository.Verify();
        }

        [Fact]
        public void Category_Should_Use_StoryRepository()
        {
            Category();

            _storyRepository.Verify();
        }

        [Fact]
        public void Category_Should_Redirect_To_Published_When_Blank_Name_Is_Specified()
        {
            var result = (RedirectToRouteResult) _controller.Category(string.Empty, 1);

            Assert.Equal("Published", result.RouteName);
        }

        [Fact]
        public void Category_Should_Throw_NotFound_Error_When_Specifed_Category_Does_Not_Exist()
        {
            _categoryRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns((ICategory) null);

            Assert.Throws<HttpException>(() => _controller.Category(CategoryName, 1));
        }

        [Fact]
        public void Upcoming_Should_Render_List_View()
        {
            var result = Upcoming();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Nadchodzące artykuły".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Nadchodzące artykuły", viewData.Subtitle);
            Assert.Equal("Cisza głucha i posucha :( - dodaj coś!", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Upcoming_Should_Use_StoryRepository()
        {
            Upcoming();

            _storyRepository.Verify();
        }

        [Fact]
        public void GetUpcoming_Should_Return_Json_Result()
        {
            var pagedResult = GetUpcoming();

            Assert.Equal(2, pagedResult.Result.Count);
            Assert.Equal(10, pagedResult.Total);
        }

        [Fact]
        public void GetUpcoming_Should_Use_StoryRepository()
        {
            GetUpcoming();

            _storyRepository.Verify();
        }

        [Fact]
        public void GetUpcoming_Should_Log_Exception()
        {
            GetUpcomingOnException();

            log.Verify();
        }

        [Fact]
        public void GetUpcoming_Should_Return_Empty_Result_When_Exception_Occurrs()
        {
            var pagedResult = GetUpcomingOnException();

            Assert.Empty(pagedResult.Result);
            Assert.Equal(0, pagedResult.Total);
        }

        [Fact]
        public void New_Should_Render_List_View()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            var result = New();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Nowe artykuły".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Nowe", viewData.Subtitle);
            Assert.Equal("Brak nowych artykułów.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void New_Should_Use_StoryRepository()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            New();

            _storyRepository.Verify();
        }

        [Fact]
        public void New_Should_Render_Error_Message_When_User_Does_Not_Have_Permission()
        {
            var result = New();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("Nie masz uprawnień do przeglądania nowych artykułów.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Unapproved_Should_Render_List_View()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            var result = Unapproved();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Niezatwierdzone artykuły".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Niezatwierdzone", viewData.Subtitle);
            Assert.Equal("Brak niezatwierdzonych artykułów.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Unapproved_Should_Use_StoryRepository()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            Unapproved();

            _storyRepository.Verify();
        }

        [Fact]
        public void Unapproved_Should_Render_Error_Message_When_User_Does_Not_Have_Permission()
        {
            var result = Unapproved();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("Nie masz uprawnień do przeglądania niezatwierdzonych artykułów.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Publishable_Should_Render_List_View()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            var result = Publishable();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Do opublikowania".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Do opublikowania", viewData.Subtitle);
            Assert.Equal("Brak artykułów do opublikowania.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Publishable_Should_Use_StoryRepository()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            Publishable();

            _storyRepository.Verify();
        }

        [Fact]
        public void Publishable_Should_Render_Error_Message_When_User_Does_Not_Have_Permission()
        {
            var result = Publishable();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("Nie masz uprawnień do przeglądania artykułów do opublikowania.", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Tags_Should_Render_List_View()
        {
            var result = Tags();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Artykuły z tagiem {1}".FormatWith(settings.Object.SiteTitle, TagName), viewData.Title);
            Assert.Equal(TagName, viewData.Subtitle);
            Assert.Equal("Brak artykułów z \"{0}\".".FormatWith(TagName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Tags_Should_Use_TagRepository()
        {
            Tags();

            _tagRepository.Verify();
        }

        [Fact]
        public void Tags_Should_Use_StoryRepository()
        {
            Tags();

            _storyRepository.Verify();
        }

        [Fact]
        public void Tags_Should_Redirect_To_Published_When_Blank_Name_Is_Specified()
        {
            var result = (RedirectToRouteResult)_controller.Tags(string.Empty, 1);

            Assert.Equal("Published", result.RouteName);
        }

        [Fact]
        public void Tags_Should_Throw_NotFound_Error_When_Specifed_Tag_Does_Not_Exist()
        {
            _tagRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns((ITag)null);

            Assert.Throws<HttpException>(() => _controller.Tags(TagName, 1));
        }

        [Fact]
        public void Search_Should_Render_List_View()
        {
            var result = Search();
            var viewData = (StoryListViewData)(result).ViewData.Model;

            Assert.Equal("List", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal("{0} - Wyniki wyszukiwania dla \"Linq\"".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.Equal("Wyniki wyszukiwania : Linq", viewData.Subtitle);
            Assert.Equal("Brak artykułów dla \"Linq\".", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Search_Should_Use_StoryRepository()
        {
            Search();

            _storyRepository.Verify();
        }

        [Fact]
        public void Search_Should_Redirect_To_Published_When_Blank_Query_Is_Specified()
        {
            var result = (RedirectToRouteResult) _controller.Search(string.Empty, null);

            Assert.Equal("Published", result.RouteName);
        }

        [Fact]
        public void Detail_Should_Render_Default_View()
        {
            var result = Detail();
            var viewData = (StoryDetailViewData) result.ViewData.Model;

            Assert.Equal(string.Empty, result.ViewName);
            Assert.Equal("{0} - Dummy Story".FormatWith(settings.Object.SiteTitle), viewData.Title);
            Assert.NotNull(viewData.Story);
        }

        [Fact]
        public void Detail_Should_Use_StoryRepository()
        {
            Detail();

            _storyRepository.Verify();
        }

        [Fact]
        public void Detail_Should_Throw_NotFound_Error_When_Specifed_Story_Does_Not_Exist()
        {
            _storyRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns((IStory) null);

            Assert.Throws<HttpException>(() => _controller.Detail("Dummy-Story"));
        }

        [Fact]
        public void Detail_Should_Redirect_To_Published_When_Blank_Name_Is_Specified()
        {
            var result = (RedirectToRouteResult) _controller.Detail(string.Empty);

            Assert.Equal("Published", result.RouteName);
        }

        [Fact]
        public void Submit_Should_Render_New_View_When_Story_Does_Not_Exist()
        {
            var result = SubmitNew();
            var viewData = (StoryContentViewData) result.ViewData.Model;

            Assert.Equal("New", result.ViewName);
            Assert.Equal("Dummy Story", viewData.Title);
        }

        [Fact]
        public void Submit_Should_Use_StoryRepository()
        {
            SubmitNew();

            _storyRepository.Verify();
        }

        [Fact]
        public void Submit_Should_Use_ContentService()
        {
            SubmitNew();

            _contentService.Verify();
        }

        [Fact]
        public void Submit_Should_Redirect_To_Detail_When_Story_Already_Exists()
        {
            var story = new Mock<IStory>();

            story.SetupGet(s => s.UniqueName).Returns("Dummy-Story");

            _storyRepository.Setup(r => r.FindByUrl(It.IsAny<string>())).Returns(story.Object);

            var result = (RedirectToRouteResult) _controller.Submit("http://story.com", null);

            Assert.Equal("Detail", result.RouteName);
        }

        [Fact]
        public void Retrieve_Should_Return_Story_Content_When_Story_Does_Not_Exist()
        {
            var result = (JsonContentViewData) Retrieve(new StoryContent("Dummy Story", "Dummy Description", "http://trackback.com"));

            Assert.Equal("Dummy Story", result.title);
            Assert.Equal("Dummy Description", result.description);
            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Retrieve_Should_Return_Story_Content_When_Url_Does_Not_Exist()
        {
            var result = Retrieve(StoryContent.Empty);

            Assert.Equal("Podany Url nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void Retrieve_Should_Use_StoryRepository()
        {
            Retrieve(StoryContent.Empty);

            _storyRepository.Verify();
        }

        [Fact]
        public void Retrive_Should_Use_Content_Service()
        {
            Retrieve(StoryContent.Empty);

            _contentService.Verify();
        }

        [Fact]
        public void Retrive_Should_Log_Exception()
        {
            _storyRepository.Setup(r => r.FindByUrl(It.IsAny<string>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Retrieve("http://astory.com");

            log.Verify();
        }

        [Fact]
        public void Retrive_Should_Return_Error_When_Story_Already_Exists()
        {
            var story = new Mock<IStory>();
            story.SetupGet(s => s.UniqueName).Returns("Dummy-Story");

            _storyRepository.Setup(r => r.FindByUrl(It.IsAny<string>())).Returns(story.Object);

            var viewData = (JsonContentViewData) ((JsonResult) _controller.Retrieve("http://astory.com")).Data;

            Assert.True(viewData.alreadyExists);
            Assert.True(viewData.existingUrl.Length > 0);
        }

        [Fact]
        public void Retrive_Should_Return_Error_When_Invalid_Url_Is_Specified()
        {
            var result = (JsonViewData) ((JsonResult) _controller.Retrieve("foo")).Data;

            Assert.Equal("Niepoprawny format Url.", result.errorMessage);
        }

        [Fact]
        public void Retrive_Should_Return_Error_When_Blank_Url_Is_Specified()
        {
            var result = (JsonViewData)((JsonResult)_controller.Retrieve(string.Empty)).Data;

            Assert.Equal("Url nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Submit_Should_Add_New_Story()
        {
            var result = Submit();

            Assert.True(result.isSuccessful);
            Assert.True(result.url.Length > 0);
        }
        [Fact]
        public void Submit_Should_Add_New_Story_Without_Captcha_When_CurrentScore_Is_High()
        {
            SetupCaptcha(false, "");
            var user = new Mock<IUser>();
            user.SetupGet(u => u.CurrentScore).Returns(10000);
            SetCurrentUser(user, Roles.User);
            _storyService.Setup(s => s.Create(It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NameValueCollection>(), It.IsAny<Func<IStory, string>>())).Returns(new StoryCreateResult { DetailUrl = "/Dummy-Story" }).Verifiable();
            var result = (JsonCreateViewData) ((JsonResult) _controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;
            Assert.True(result.isSuccessful);
            Assert.True(result.url.Length > 0);

        }

        [Fact]
        public void Submit_Should_Use_StoryService()
        {
            Submit();

            _storyService.Verify();
        }

        [Fact]
        public void Submit_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            SetupCaptcha();
            _storyService.Setup(s => s.Create(It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NameValueCollection>(), It.IsAny<Func<IStory, string>>())).Throws<InvalidOperationException>();

            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();
            _controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test");

            log.Verify();
        }

        [Fact]
        public void Submit_Should_Return_Error_When_Captcha_Does_Not_Match()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);
            SetupCaptcha();

            //_reCAPTCHA.Setup(c => c.Validate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _controller.CaptchaValidatorFunc = s => false;

            var result = (JsonViewData)((JsonResult)_controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Pole Captcha jest nieprawidłowe", result.errorMessage);
        }

        [Fact]
        public void Submit_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            SetupCaptcha();

            var result = (JsonViewData)((JsonResult)_controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Submit_Should_Return_Error_When_Captcha_Response_Is_Blank()
        {
            SetupCaptcha(false, "");
            var result = (JsonViewData)((JsonResult)_controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Pole Captcha nie może być puste.", result.errorMessage);
        }

        [Fact]
        public void Post_Should_Return_Error_When_Captcha_Challenge_Is_Blank()
        {
            SetupCaptcha(false, "");

            var result = (JsonViewData)((JsonResult)_controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Pole Captcha nie może być puste.", result.errorMessage);
        }

        [Fact]
        public void Click_Should_View_Story()
        {
            var result = Click(new Mock<IStory>().Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Click_Should_Use_StoryRepository()
        {
            Click(new Mock<IStory>().Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void Click_Should_Use_StoryService()
        {
            Click(new Mock<IStory>().Object);

            _storyService.Verify();
        }

        [Fact]
        public void Click_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Click(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void Click_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var result = Click(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void Click_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData) ((JsonResult) _controller.Click("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Click_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Click(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Promote_Story()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanPromote(It.IsAny<IUser>())).Returns(true);

            var result = Promote(story.Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Promote_Should_Use_StoryRepository()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanPromote(It.IsAny<IUser>())).Returns(true);

            Promote(story.Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void Promote_Should_Use_StoryService()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanPromote(It.IsAny<IUser>())).Returns(true);

            Promote(story.Object);

            _storyService.Verify();
        }

        [Fact]
        public void Promote_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Promote(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void Promote_Should_Return_Error_When_User_Cannot_Promote()
        {
            var story = new Mock<IStory>();

            var result = Promote(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie możesz promować tego artykułu.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Return_Error_When_User_Has_Promoted()
        {
            var story = new Mock<IStory>();

            story.Setup(s => s.HasPromoted(It.IsAny<IUser>())).Returns(true);

            var result = Promote(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Już wypromowałeś ten artykuł.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var result = Promote(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Promote(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Promote("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Promote_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Promote(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Demote_Should_Demote_Story()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanDemote(It.IsAny<IUser>())).Returns(true);

            var result = Demote(story.Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Demote_Should_Use_StoryRepository()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanDemote(It.IsAny<IUser>())).Returns(true);

            Demote(story.Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void Demote_Should_Use_StoryService()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanDemote(It.IsAny<IUser>())).Returns(true);

            Demote(story.Object);

            _storyService.Verify();
        }

        [Fact]
        public void Demote_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Demote(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void Demote_Should_Return_Error_When_User_Cannot_Demote()
        {
            var story = new Mock<IStory>();

            var result = Demote(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie możesz degradować tego artykułu.", result.errorMessage);
        }

        [Fact]
        public void Demote_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var result = Demote(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void Demote_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Demote(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Demote_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Demote("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Demote_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Demote(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Mark_Story_As_Spam()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanMarkAsSpam(It.IsAny<IUser>())).Returns(true);

            var result = MarkAsSpam(story.Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void MarkAsSpam_Should_Use_StoryRepository()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanMarkAsSpam(It.IsAny<IUser>())).Returns(true);

            MarkAsSpam(story.Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void MarkAsSpam_Should_Use_StoryService()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.CanMarkAsSpam(It.IsAny<IUser>())).Returns(true);

            MarkAsSpam(story.Object);

            _storyService.Verify();
        }

        [Fact]
        public void MarkAsSpam_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.MarkAsSpam(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_User_Cannot_MarkAsSpam()
        {
            var story = new Mock<IStory>();

            var result = MarkAsSpam(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do zaznaczania tego artykułu jako spam.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_User_Has_Promoted()
        {
            var story = new Mock<IStory>();

            story.Setup(s => s.HasMarkedAsSpam(It.IsAny<IUser>())).Returns(true);

            var result = MarkAsSpam(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Już zaznaczyłeś ten artykuł jako spam.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var result = MarkAsSpam(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.MarkAsSpam(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Promote("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void MarkAsSpam_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.MarkAsSpam(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Publish_Should_Publish_Stories()
        {
            var result = Publish();

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Publish_Should_Use_StoryService()
        {
            Publish();

            _storyService.Verify();
        }

        [Fact]
        public void Publish_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Administrator);

            _storyService.Setup(s => s.Publish()).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Publish();

            log.Verify();
        }

        [Fact]
        public void Publish_Should_Return_Error_When_User_Is_Not_Administrator()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            var result = (JsonViewData) ((JsonResult) _controller.Publish()).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void Publish_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Publish()).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void GetStory_Should_Return_Story()
        {
            var story = GetStory();

            Assert.NotNull(story);
        }

        [Fact]
        public void GetStory_Should_Use_StoryRepository()
        {
            GetStory();

            _storyRepository.Verify();
        }

        [Fact]
        public void GetStory_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.GetStory(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void GetStory_Should_Return_Error_When_Story_Does_Not_Exists()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns((IStory) null);

            var result = (JsonViewData) ((JsonResult) _controller.GetStory(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void GetStory_Should_Return_Error_When_User_Can_Not_Moderate()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            var result = (JsonViewData)((JsonResult)_controller.GetStory(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void GetStory_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.GetStory(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void GetStory_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.GetStory("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void GetStory_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.GetStory(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Update_Should_Update_The_Story()
        {
            var result = Update(new Mock<IStory>().Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Update_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();

            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Update(Guid.NewGuid().Shrink(), "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar");

            log.Verify();
        }

        [Fact]
        public void Update_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var resuslt = Update(null);

            Assert.False(resuslt.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", resuslt.errorMessage);
        }

        [Fact]
        public void Update_Should_Return_Error_When_User_Can_Not_Moderate()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            var result = (JsonViewData) ((JsonResult) _controller.Update(Guid.NewGuid().Shrink(), "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void Update_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Update(Guid.NewGuid().Shrink(), "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Update_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Update("foobar", "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Update_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Update(string.Empty, "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Delete_Should_Delete_The_Story()
        {
            var result = Delete(new Mock<IStory>().Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Delete_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();

            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Delete(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void Delete_Should_Return_Error_When_Story_Does_Not_Exist()
        {
            var resuslt = Delete(null);

            Assert.False(resuslt.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", resuslt.errorMessage);
        }

        [Fact]
        public void Delete_Should_Return_Error_When_User_Can_Not_Moderate()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            var result = (JsonViewData)((JsonResult)_controller.Delete(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void Delete_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Delete(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Delete_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Delete("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Delete_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Delete(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Discard_The_Story_As_Spam()
        {
            var result = Approve(new Mock<IStory>().Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void Approve_Should_Use_StoryRepository()
        {
            Approve(new Mock<IStory>().Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void Approve_Should_Use_StoryService()
        {
            Approve(new Mock<IStory>().Object);

            _storyService.Verify();
        }

        [Fact]
        public void Approve_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.Approve(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void Approve_Should_Return_Error_When_Story_Is_Already_Approved()
        {
            var story = new Mock<IStory>();

            story.SetupGet(s => s.ApprovedAt).Returns(SystemTime.Now().AddDays(-1));

            var result = Approve(story.Object);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł już został zatwierdzony.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Return_Error_When_Story_Does_Not_Exists()
        {
            var result = Approve(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Return_Error_When_User_Can_Not_Moderate()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            var result = (JsonViewData)((JsonResult)_controller.Approve(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.Approve(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.Approve("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void Approve_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.Approve(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void ConfirmSpam_Should_Confirm_The_Story_As_Spam()
        {
            var result = ConfirmSpam(new Mock<IStory>().Object);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public void ConfirmSpam_Should_Use_StoryRepository()
        {
            ConfirmSpam(new Mock<IStory>().Object);

            _storyRepository.Verify();
        }

        [Fact]
        public void ConfirmSpam_Should_Use_StoryService()
        {
            ConfirmSpam(new Mock<IStory>().Object);

            _storyService.Verify();
        }

        [Fact]
        public void ConfirmSpam_Should_Log_Exception()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Throws<InvalidOperationException>();
            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            _controller.ConfirmSpam(Guid.NewGuid().Shrink());

            log.Verify();
        }

        [Fact]
        public void ConfirmSpam_Should_Return_Error_When_Story_Does_Not_Exists()
        {
            var result = ConfirmSpam(null);

            Assert.False(result.isSuccessful);
            Assert.Equal("Podany artykuł nie istnieje.", result.errorMessage);
        }

        [Fact]
        public void ConfirmSpam_Should_Return_Error_When_User_Can_Not_Moderate()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            var result = (JsonViewData)((JsonResult)_controller.ConfirmSpam(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie masz uprawnień do wywoływania tej metody.", result.errorMessage);
        }

        [Fact]
        public void ConfirmSpam_Should_Return_Error_When_User_Is_Not_Authenticated()
        {
            var result = (JsonViewData)((JsonResult)_controller.ConfirmSpam(Guid.NewGuid().Shrink())).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Nie jesteś zalogowany.", result.errorMessage);
        }

        [Fact]
        public void ConfirmSpam_Should_Return_Error_When_StoryId_Is_Invalid()
        {
            var result = (JsonViewData)((JsonResult)_controller.ConfirmSpam("foobar")).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Niepoprawny identyfikator artykułu.", result.errorMessage);
        }

        [Fact]
        public void ConfirmSpam_Should_Return_Error_When_StoryId_Is_Blank()
        {
            var result = (JsonViewData)((JsonResult)_controller.ConfirmSpam(string.Empty)).Data;

            Assert.False(result.isSuccessful);
            Assert.Equal("Identyfikator artykułu nie może być pusty.", result.errorMessage);
        }

        [Fact]
        public void PromotedBy_Should_Render_UserStoryList_View()
        {
            var result = PromotedBy();
            var viewData = (StoryListUserViewData) result.ViewData.Model;

            Assert.Equal("UserStoryList", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal(UserDetailTab.Promoted, viewData.SelectedTab);
            Assert.Equal("Brak artykułów wypromowanych przez \"{0}\".".FormatWith(UserName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void PromotedBy_Should_UserRepository()
        {
            PromotedBy();

            _userRepository.Verify();
        }

        [Fact]
        public void PromotedBy_Should_StoryRepository()
        {
            PromotedBy();

            _storyRepository.Verify();
        }

        [Fact]
        public void PostedBy_Should_Render_UserStoryList_View()
        {
            var result = PostedBy();
            var viewData = (StoryListUserViewData)result.ViewData.Model;

            Assert.Equal("UserStoryList", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal(UserDetailTab.Posted, viewData.SelectedTab);
            Assert.Equal("Brak artykułów opublikowanych przez \"{0}\".".FormatWith(UserName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void PostedBy_Should_UserRepository()
        {
            PostedBy();

            _userRepository.Verify();
        }

        [Fact]
        public void PostedBy_Should_StoryRepository()
        {
            PostedBy();

            _storyRepository.Verify();
        }

        [Fact]
        public void CommentedBy_Should_Render_UserStoryList_View()
        {
            var result = CommentedBy();
            var viewData = (StoryListUserViewData)result.ViewData.Model;

            Assert.Equal("UserStoryList", result.ViewName);
            Assert.Equal(0, viewData.TotalStoryCount);
            Assert.Empty(viewData.Stories);
            Assert.Equal(UserDetailTab.Commented, viewData.SelectedTab);
            Assert.Equal("Brak artykułów skomentowanych przez \"{0}\".".FormatWith(UserName), viewData.NoStoryExistMessage);
        }

        [Fact]
        public void CommentedBy_Should_UserRepository()
        {
            CommentedBy();

            _userRepository.Verify();
        }

        [Fact]
        public void CommentedBy_Should_StoryRepository()
        {
            CommentedBy();

            _storyRepository.Verify();
        }

        [Fact]
        public void Weekly_Should_Render_List()
        {
            _storyService.Setup(x => x.FindWeekly(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new List<IStory>());
            var result = Weekly(38, 2016);

            Assert.Equal("List", result.ViewName);
        }

        [Fact]
        public void Weekly_WithWeekInTheFuture_ShouldReturnError()
        {
            SystemTime.Now = () => new DateTime(2016, 9, 26);
            var result = Weekly(50, 2016);

            var viewData = (StoryListViewData)result.ViewData.Model;

            Assert.Equal("Nieprawidłowy nr tygodnia w roku", viewData.NoStoryExistMessage);
        }

        [Fact]
        public void Weekly_OnMondayMidnight_DoesNotThrowException()
        {
            _storyService.Setup(x => x.FindWeekly(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(new List<IStory>());
            SystemTime.Now = () => new DateTime(2016,9,26, 0,0,0);
            Assert.DoesNotThrow(() => Weekly(38, 2016));
        }

        private ViewResult Published()
        {
            _storyRepository.Setup(r => r.FindPublished(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.Published(1);
        }

        private PagedResult<StorySummary> GetPublished()
        {
            var story1 = new Mock<IStory>();

            story1.SetupGet(s => s.Id).Returns(new Guid("AD46E159-B6A3-4A69-BC68-8A990CDAC4BA"));
            story1.SetupGet(s => s.Title).Returns("TestStory1");
            story1.SetupGet(s => s.UniqueName).Returns("TestStory1");
            story1.SetupGet(s => s.Url).Returns("http://story1.com");
            story1.SetupGet(s => s.TextDescription).Returns("TestStory1 Description");

            var story2 = new Mock<IStory>();

            story2.SetupGet(s => s.Id).Returns(new Guid("20559AB2-4769-4EB3-AB23-D460412BF164"));
            story2.SetupGet(s => s.Title).Returns("TestStory2");
            story2.SetupGet(s => s.UniqueName).Returns("TestStory2");
            story2.SetupGet(s => s.Url).Returns("http://story2.com");
            story2.SetupGet(s => s.TextDescription).Returns("TestStory2 Description");

            _httpContext.HttpRequest.SetupGet(r => r.QueryString).Returns(new NameValueCollection { { "start", "20" }, { "max", "60" } });
            _storyRepository.Setup(r => r.FindPublished(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>(new List<IStory> { story1.Object, story2.Object }, 10)).Verifiable();

            return (PagedResult<StorySummary>)((JsonResult)_controller.GetPublished(null, 70)).Data;
        }

        private PagedResult<StorySummary> GetPublishedOnException()
        {
            _storyRepository.Setup(r => r.FindPublished(It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            return (PagedResult<StorySummary>)((JsonResult)_controller.GetPublished(10, 70)).Data;
        }

        private ViewResult Category(int page = 1)
        {
            var category = new Mock<ICategory>();

            category.SetupGet(c => c.Id).Returns(Guid.NewGuid());
            category.SetupGet(c => c.Name).Returns(CategoryName);
            category.SetupGet(c => c.UniqueName).Returns(CategoryName);

            _categoryRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns(category.Object).Verifiable();
            _storyRepository.Setup(r => r.FindPublishedByCategory(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult) _controller.Category(CategoryName, page);
        }

        private ViewResult Upcoming()
        {
            _storyRepository.Setup(r => r.FindUpcoming(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.Upcoming(1);
        }

        private PagedResult<StorySummary> GetUpcoming()
        {
            var story1 = new Mock<IStory>();

            story1.SetupGet(s => s.Id).Returns(new Guid("5F77DB1D-74D3-44D7-9C8C-97E1729C040C"));
            story1.SetupGet(s => s.Title).Returns("TestStory1");
            story1.SetupGet(s => s.UniqueName).Returns("TestStory1");
            story1.SetupGet(s => s.Url).Returns("http://story1.com");
            story1.SetupGet(s => s.TextDescription).Returns("TestStory1 Description");

            var story2 = new Mock<IStory>();

            story2.SetupGet(s => s.Id).Returns(new Guid("AEAE691E-C05C-4FB0-A1D2-FFE18ED4CD01"));
            story2.SetupGet(s => s.Title).Returns("TestStory2");
            story2.SetupGet(s => s.UniqueName).Returns("TestStory2");
            story2.SetupGet(s => s.Url).Returns("http://story2.com");
            story2.SetupGet(s => s.TextDescription).Returns("TestStory2 Description");

            _storyRepository.Setup(r => r.FindUpcoming(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>(new List<IStory> { story1.Object, story2.Object }, 10)).Verifiable();

            return (PagedResult<StorySummary>)((JsonResult)_controller.GetUpcoming(null, null)).Data;
        }

        private PagedResult<StorySummary> GetUpcomingOnException()
        {
            _storyRepository.Setup(r => r.FindUpcoming(It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            log.Setup(l => l.Exception(It.IsAny<Exception>())).Verifiable();

            return (PagedResult<StorySummary>)((JsonResult)_controller.GetUpcoming(10, 70)).Data;
        }

        private ViewResult New()
        {
            _storyRepository.Setup(r => r.FindNew(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.New(1);
        }

        private ViewResult Unapproved()
        {
            _storyRepository.Setup(r => r.FindUnapproved(It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.Unapproved(1);
        }

        private ViewResult Publishable()
        {
            _storyRepository.Setup(r => r.FindPublishable(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult) _controller.Publishable(1);
        }

        private ViewResult Tags()
        {
            var tag = new Mock<ITag>();

            tag.SetupGet(c => c.Id).Returns(Guid.NewGuid());
            tag.SetupGet(c => c.Name).Returns(TagName);
            tag.SetupGet(c => c.UniqueName).Returns(TagName);

            _tagRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns(tag.Object).Verifiable();
            _storyRepository.Setup(r => r.FindByTag(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.Tags(TagName, 1);
        }

        private ViewResult Search()
        {
            _storyRepository.Setup(r => r.Search(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult) _controller.Search("Linq", 1);
        }

        private ViewResult Detail()
        {
            var story = new Mock<IStory>();
            story.Setup(s => s.Title).Returns("Dummy Story");
            story.Setup(s => s.TextDescription).Returns("This is the dummy description of the dummy story");

            _storyRepository.Setup(r => r.FindByUniqueName(It.IsAny<string>())).Returns(story.Object).Verifiable();

            return (ViewResult) _controller.Detail("Dummy-Story");
        }

        private ViewResult SubmitNew()
        {
            _storyRepository.Setup(r => r.FindByUrl(It.IsAny<string>())).Returns((IStory) null).Verifiable();
            _contentService.Setup(s => s.Get(It.IsAny<string>())).Returns(new StoryContent("Dummy Story", "Dummy Description", "http://trackback.com")).Verifiable();

            return (ViewResult) _controller.Submit("http://astory.com", string.Empty);
        }

        private JsonViewData Retrieve(StoryContent content)
        {
            _storyRepository.Setup(r => r.FindByUrl(It.IsAny<string>())).Returns((IStory) null).Verifiable();
            _contentService.Setup(s => s.Get(It.IsAny<string>())).Returns(content).Verifiable();

            return (JsonViewData) ((JsonResult) _controller.Retrieve("http://astory.com")).Data;
        }

        private JsonCreateViewData Submit()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);
            SetupCaptcha();

            _storyService.Setup(s => s.Create(It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NameValueCollection>(), It.IsAny<Func<IStory, string>>())).Returns(new StoryCreateResult { DetailUrl = "/Dummy-Story" }).Verifiable();

            return (JsonCreateViewData) ((JsonResult) _controller.Submit("http://astory.com", "Dummy story", "Dummy", "Dummy Story Description", "Dummy, Test")).Data;
        }

        private JsonViewData Click(IStory story)
        {
            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.View(It.IsAny<IStory>(), It.IsAny<IUser>(), It.IsAny<string>())).Verifiable();

            return (JsonViewData) ((JsonResult) _controller.Click(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData Promote(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Promote(It.IsAny<IStory>(), It.IsAny<IUser>(), It.IsAny<string>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.Promote(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData Demote(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Demote(It.IsAny<IStory>(), It.IsAny<IUser>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.Demote(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData MarkAsSpam(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.User);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.MarkAsSpam(It.IsAny<IStory>(), It.IsAny<string>(), It.IsAny<IUser>(), It.IsAny<string>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.MarkAsSpam(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData Publish()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Administrator);
            _storyService.Setup(s => s.Publish()).Verifiable();

            return (JsonViewData) ((JsonResult) _controller.Publish()).Data;
        }

        private object GetStory()
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            var category = new Mock<ICategory>();

            category.SetupGet(c => c.UniqueName).Returns("Dummy");

            var tag1 = new Mock<ITag>();

            tag1.SetupGet(t => t.Name).Returns("Dummy1");

            var tag2 = new Mock<ITag>();

            tag2.SetupGet(t => t.Name).Returns("Dummy2");

            var story = new Mock<IStory>();

            story.SetupGet(s => s.Id).Returns(Guid.NewGuid());
            story.SetupGet(s => s.UniqueName).Returns("Dummy-Stroy");
            story.SetupGet(s => s.CreatedAt).Returns(SystemTime.Now());
            story.SetupGet(s => s.HtmlDescription).Returns("<p>This is a summy description</p>");
            story.SetupGet(s => s.BelongsTo).Returns(category.Object);
            story.SetupGet(s => s.Tags).Returns(new[] { tag1.Object, tag2.Object });

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story.Object).Verifiable();

            return ((JsonResult) _controller.GetStory(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData Update(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Update(It.IsAny<IStory>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.Update(Guid.NewGuid().Shrink(), "Dummy-Story", SystemTime.Now(), "Dummy Story", "Dummy", "Dummy Description", "foo,bar")).Data;
        }

        private JsonViewData Delete(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Delete(It.IsAny<IStory>(), It.IsAny<IUser>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.Delete(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData Approve(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Approve(It.IsAny<IStory>(), It.IsAny<string>(), It.IsAny<IUser>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.Approve(Guid.NewGuid().Shrink())).Data;
        }

        private JsonViewData ConfirmSpam(IStory story)
        {
            SetCurrentUser(new Mock<IUser>(), Roles.Moderator);

            _storyRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(story).Verifiable();
            _storyService.Setup(s => s.Spam(It.IsAny<IStory>(), It.IsAny<string>(), It.IsAny<IUser>())).Verifiable();

            return (JsonViewData)((JsonResult)_controller.ConfirmSpam(Guid.NewGuid().Shrink())).Data;
        }

        private ViewResult PromotedBy()
        {
            var user = new Mock<IUser>();

            user.SetupGet(u => u.UserName).Returns(UserName);

            _userRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(user.Object).Verifiable();
            _storyRepository.Setup(r => r.FindPromotedByUser(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult) _controller.PromotedBy(UserName, 1);
        }

        private ViewResult PostedBy()
        {
            var user = new Mock<IUser>();

            user.SetupGet(u => u.UserName).Returns(UserName);

            _userRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(user.Object).Verifiable();
            _storyRepository.Setup(r => r.FindPostedByUser(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.PostedBy(UserName, 1);
        }

        private ViewResult CommentedBy()
        {
            var user = new Mock<IUser>();

            user.SetupGet(u => u.UserName).Returns(UserName);

            _userRepository.Setup(r => r.FindById(It.IsAny<Guid>())).Returns(user.Object).Verifiable();
            _storyRepository.Setup(r => r.FindCommentedByUser(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<IStory>()).Verifiable();

            return (ViewResult)_controller.CommentedBy(UserName, 1);
        }

        private ViewResult Weekly(int week, int year)
        {
            var user = new Mock<IUser>();
            user.SetupGet(u => u.UserName).Returns(UserName);

            return (ViewResult)_controller.WeeklyDigest(week, year, 1);
        }

        private void SetupCaptcha(bool captchaResult = true, string captchaResponse = "bar")
        {
            var nameValue = new NameValueCollection();
            nameValue.Add("g-recaptcha-response", captchaResponse);
            _httpContext.HttpRequest.SetupGet(r => r.Params).Returns(nameValue);
            _httpContext.HttpRequest.SetupGet(r => r.UserHostAddress).Returns("192.168.0.1");
            _controller.CaptchaValidatorFunc = s => captchaResult;
        }

        private void SetCurrentUser(Mock<IUser> user, Roles role)
        {
            var userId = Guid.NewGuid();

            user.SetupGet(u => u.Id).Returns(userId);
            user.SetupGet(u => u.UserName).Returns(UserName);
            user.SetupGet(u => u.Role).Returns(role);

            _httpContext.User.Identity.SetupGet(i => i.Name).Returns(UserName);
            _httpContext.User.Identity.SetupGet(i => i.IsAuthenticated).Returns(true);

            _userRepository.Setup(r => r.FindByUserName(UserName)).Returns(user.Object);
        }
    }
}