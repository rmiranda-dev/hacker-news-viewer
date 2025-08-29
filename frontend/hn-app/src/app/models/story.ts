export interface Story {
  id: number;
  title: string | null;
  url: string | null;
  by: string;
  time: number;
}

export interface PagedStories {
  total: number;
  items: Story[];
}
