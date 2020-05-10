export interface SearchResults {
  query: string;
  results: SearchResult[];
}

export interface SearchResult {
  identifier: string;
  title: string;
  matches: string[];
  keywords: string[];
  url: string;
  type: string;
}

export interface SearchSuggestions {
  query: string;
  results: SearchSuggestion[];
}

export interface SearchSuggestion {
  text: string;
  highlight: string;
}

export enum StatusEnum {
  None = "none",
  Scheduled = "scheduled",
  Indexed = "indexed",
  Failed = "failed"
}

export interface DocumentStatus {
  documentId: string;
  filename: string;
  title: string;
  isOcrRequested: boolean;
  status: StatusEnum;
}