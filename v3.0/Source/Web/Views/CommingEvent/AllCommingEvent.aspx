﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteTemplate.Master" Inherits="System.Web.Mvc.ViewPage<Kigg.Web.ViewData.CommingEventsViewData>" %>

<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContentPlaceHolder">
    <div>
        <%= Html.PageHeader("Nadchodzące wydarzenia") %>
        <% Html.RenderPartial("CommingEventList", Model); %>
    </div>
    <div class="commingEventsInvitationToInformAbout">
        Jeżeli macie informację o ciekawym wydarzeniu związanym ze światem .Netu, prosimy o podanie informacji poprzez
        <a id="lnkAddEventUser" href="javascript:void(0)" onclick="Moderation.showEvent()" style="">formularz</a>. Daj nam znać, a umieścimy informację na naszej stronie. W wiadomości prześlij
        link do strony wydarzenia, oraz interesujące wg. Ciebie informacje. Inni dotNETomaniacy czekają na każde interesujące wydarzenie;)
    </div>
</asp:Content>