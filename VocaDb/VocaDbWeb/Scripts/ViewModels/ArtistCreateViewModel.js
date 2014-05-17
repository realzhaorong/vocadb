/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../DataContracts/DuplicateEntryResultContract.ts" />
/// <reference path="../Repositories/ArtistRepository.ts" />
/// <reference path="WebLinkEditViewModel.ts" />
var vdb;
(function (vdb) {
    (function (viewModels) {
        var ArtistCreateViewModel = (function () {
            function ArtistCreateViewModel(artistRepository, data) {
                var _this = this;
                this.dupeEntries = ko.observableArray([]);
                this.nameOriginal = ko.observable("");
                this.nameRomaji = ko.observable("");
                this.nameEnglish = ko.observable("");
                this.submit = function () {
                    _this.submitting(true);
                    return true;
                };
                this.submitting = ko.observable(false);
                this.webLink = new viewModels.WebLinkEditViewModel();
                if (data) {
                    this.nameOriginal(data.nameOriginal || "");
                    this.nameRomaji(data.nameRomaji || "");
                    this.nameEnglish(data.nameEnglish || "");
                }

                this.checkDuplicates = function () {
                    var term1 = _this.nameOriginal();
                    var term2 = _this.nameRomaji();
                    var term3 = _this.nameEnglish();
                    var linkUrl = _this.webLink.url();

                    artistRepository.findDuplicate({ term1: term1, term2: term2, term3: term3, linkUrl: linkUrl }, function (result) {
                        _this.dupeEntries(result);
                    });
                };
            }
            return ArtistCreateViewModel;
        })();
        viewModels.ArtistCreateViewModel = ArtistCreateViewModel;
    })(vdb.viewModels || (vdb.viewModels = {}));
    var viewModels = vdb.viewModels;
})(vdb || (vdb = {}));
