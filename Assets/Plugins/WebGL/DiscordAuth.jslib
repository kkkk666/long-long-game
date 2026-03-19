mergeInto(LibraryManager.library, {
  OpenDiscordAuthPopup: function(urlPtr) {
    var url = UTF8ToString(urlPtr);
    window.open(url, 'discord_auth', 'width=500,height=600,scrollbars=no');
  }
});
