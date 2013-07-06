
module vdb.knockoutExtensions {

    export interface AutoCompleteParams {

        acceptSelection?: (id: number, term: string) => void;

        allowCreateNew?: boolean;

        extraQueryParams?;

        height?: number;

    }

}

