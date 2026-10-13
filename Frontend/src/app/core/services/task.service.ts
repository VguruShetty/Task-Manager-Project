import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskItemDto, CreateTaskDto, UpdateTaskStatusDto, PagedResult } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/tasks`;

  getTasksByProject(projectId: string): Observable<TaskItemDto[]> {
    const params = new HttpParams()
      .set('projectId', projectId)
      .set('pageNumber', '1')
      .set('pageSize', '50');

    return this.http.get<PagedResult<TaskItemDto>>(this.baseUrl, { params }).pipe(
      map((res) => res.items)
    );
  }

  createTask(projectId: string, dto: CreateTaskDto): Observable<TaskItemDto> {
    const payload = {
      projectId: projectId,
      title: dto.title.trim(),
      description: dto.description?.trim() || null,
      priority: dto.priority,
      dueDateUtc: dto.dueDateUtc || null,
      tagIds: dto.tagIds || []
    };

    return this.http.post<TaskItemDto>(this.baseUrl, payload);
  }

  updateTaskStatus(taskId: string, dto: UpdateTaskStatusDto): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${taskId}/status`, dto);
  }

  deleteTask(taskId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${taskId}`);
  }
}