import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CdkDragDrop,
  CdkDropListGroup,
  CdkDropList,
  CdkDrag
} from '@angular/cdk/drag-drop';
import { TaskService } from '../../../core/services/task.service';
import { TaskItemDto, TaskPriority, TaskStatus } from '../../../core/models/api.models';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    CdkDropListGroup,
    CdkDropList,
    CdkDrag
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss'
})
export class ProjectDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private taskService = inject(TaskService);
  private fb = inject(FormBuilder);

  projectId = signal<string>('');
  tasks = signal<TaskItemDto[]>([]);
  isLoading = signal(false);
  showModal = signal(false);
  errorMessage = signal<string | null>(null);

  // Filter tasks into columns by status
  todoTasks = computed(() => this.tasks().filter((t) => t.status === 'Todo'));
  inProgressTasks = computed(() => this.tasks().filter((t) => t.status === 'InProgress'));
  completedTasks = computed(() => this.tasks().filter((t) => t.status === 'Completed'));

  createTaskForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: [''],
    priority: ['Medium' as TaskPriority, Validators.required]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.projectId.set(id);
      this.loadTasks();
    }
  }

  loadTasks(): void {
    this.isLoading.set(true);
    this.taskService.getTasksByProject(this.projectId()).subscribe({
      next: (data) => {
        this.tasks.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onDrop(event: CdkDragDrop<TaskItemDto[]>, targetStatus: TaskStatus): void {
    const draggedTask = event.item.data as TaskItemDto;

    // Reordering within the same column
    if (event.previousContainer === event.container) {
      return;
    }

    const previousStatus = draggedTask.status;

    // Optimistic UI update: update signal state immediately
    this.tasks.update((current) =>
      current.map((t) => (t.id === draggedTask.id ? { ...t, status: targetStatus } : t))
    );

    // Persist new status to backend: PATCH /api/tasks/{id}/status
    this.taskService.updateTaskStatus(draggedTask.id, { status: targetStatus }).subscribe({
      error: (err) => {
        // Rollback state on error
        this.tasks.update((current) =>
          current.map((t) => (t.id === draggedTask.id ? { ...t, status: previousStatus } : t))
        );
        this.errorMessage.set('Failed to update task status.');
      }
    });
  }

  deleteTask(taskId: string): void {
    if (!confirm('Are you sure you want to delete this task?')) return;

    this.taskService.deleteTask(taskId).subscribe({
      next: () => {
        this.tasks.update((current) => current.filter((t) => t.id !== taskId));
      }
    });
  }

  submitCreate(): void {
    if (this.createTaskForm.invalid) {
      this.createTaskForm.markAllAsTouched();
      return;
    }

    const val = this.createTaskForm.getRawValue();
    this.taskService
      .createTask(this.projectId(), {
        title: val.title!,
        description: val.description || undefined,
        priority: val.priority!
      })
      .subscribe({
        next: (newTask) => {
          this.tasks.update((prev) => [newTask, ...prev]);
          this.showModal.set(false);
          this.createTaskForm.reset({ priority: 'Medium' });
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || 'Failed to create task.');
        }
      });
  }
}