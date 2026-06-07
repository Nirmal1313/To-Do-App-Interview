export interface SubTask {
  id: string | null;
  title: string;
  isCompleted: boolean;
  parentTaskId: string;
}

export interface SubTaskDto {
  id?: string | null;
  title: string;
  isCompleted: boolean;
  parentTaskId: string;
}
