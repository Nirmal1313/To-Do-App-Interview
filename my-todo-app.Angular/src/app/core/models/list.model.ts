export interface TodoList {
  id: string;
  name: string;
  color: string | null;
  taskCount: number;
}

export interface ToDoListDto {
  id?: string | null;
  name: string;
  color?: string | null;
}
