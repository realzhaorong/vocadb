/// <reference path="EntryRefContract.ts" />

module vdb.dataContracts {

    export interface ArtistContract {

        additionalNames: string;

        id: number;

        name: string;

    }

    export interface DuplicateEntryResultContract {

        entry: EntryRefWithNameContract;

        matchProperty: string;

    }

    export interface EntryNameContract {

        additionalNames: string;

        displayName: string;

    }

    export interface EntryRefWithNameContract extends EntryRefContract {

        entryTypeName: string;

        name: EntryNameContract;

    }

    export interface NewSongCheckResultContract {

        artists: ArtistContract[];

        matches: DuplicateEntryResultContract[];

        songType: string;

        title: string;

    }

}