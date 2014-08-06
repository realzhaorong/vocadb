/// <reference path="../DataContracts/TranslatedEnumField.ts" />
/// <reference path="../DataContracts/WebLinkContract.ts" />
/// <reference path="WebLinksEditViewModel.ts" />

module vdb.viewModels {

	import cls = vdb.models;
	import dc = vdb.dataContracts;
	import rep = vdb.repositories;

    export class ArtistEditViewModel {

		public artistType: KnockoutComputed<cls.artists.ArtistType>;
		public artistTypeStr: KnockoutObservable<string>;
		public allowBaseVoicebank: KnockoutComputed<boolean>;
		public baseVoicebank: artists.ArtistLinkViewModel;
		public baseVoicebankSearchParams: vdb.knockoutExtensions.AutoCompleteParams;
		public description: KnockoutObservable<string>;
		public hasValidationErrors: KnockoutComputed<boolean>;
		public id: number;

		public names: globalization.NamesEditViewModel;

        public submit = () => {
            this.submitting(true);
            return true;
        }

        public submitting = ko.observable(false);
		public validationExpanded = ko.observable(false);
		public validationError_needReferences: KnockoutComputed<boolean>;
		public validationError_needType: KnockoutComputed<boolean>;
		public validationError_unspecifiedNames: KnockoutComputed<boolean>;
        public webLinks: WebLinksEditViewModel;

        constructor(artistRepo: rep.ArtistRepository, webLinkCategories: vdb.dataContracts.TranslatedEnumField[], data: ArtistEdit) {

			this.artistTypeStr = ko.observable(data.artistType);
			this.artistType = ko.computed(() => cls.artists.ArtistType[this.artistTypeStr()]);
			this.allowBaseVoicebank = ko.computed(() => helpers.ArtistHelper.isVocalistType(this.artistType()) || this.artistType() == cls.artists.ArtistType.OtherIndividual);
			this.baseVoicebank = viewModels.artists.ArtistLinkViewModel.createFromContract(artistRepo, data.baseVoicebank);
			this.description = ko.observable(data.description);
			this.id = data.id;
			this.names = globalization.NamesEditViewModel.fromContracts(data.names);
            this.webLinks = new WebLinksEditViewModel(data.webLinks, webLinkCategories);
    
			this.baseVoicebankSearchParams = {
				acceptSelection: this.baseVoicebank.selectArtist,
				extraQueryParams: { artistTypes: "Vocaloid,UTAU,OtherVocalist,OtherVoiceSynthesizer,Unknown" },
				filter: (item) => item.Id != this.id,
			};

			this.validationError_needReferences = ko.computed(() => (this.description() == null || this.description().length) == 0 && this.webLinks.webLinks().length == 0);
			this.validationError_needType = ko.computed(() => this.artistType() == cls.artists.ArtistType.Unknown);
			this.validationError_unspecifiedNames = ko.computed(() => !this.names.hasNameWithLanguage());

			this.hasValidationErrors = ko.computed(() =>
				this.validationError_needReferences() ||
				this.validationError_needType() ||
				this.validationError_unspecifiedNames()
			);
			    
        }

    }

    export interface ArtistEdit {

		artistType: string;

		baseVoicebank: dc.ArtistContract;

		description: string;

		id: number;

		names: dc.globalization.LocalizedStringWithIdContract[];

        webLinks: vdb.dataContracts.WebLinkContract[];

    }

}