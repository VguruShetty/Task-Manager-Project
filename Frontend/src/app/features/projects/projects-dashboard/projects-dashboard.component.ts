import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProjectResponseDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-projects-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: '../projects-dashboard/projects-dashboard.component.html',
  styleUrls: ['./projects-dashboard.component.scss']
})
export class ProjectsDashboardComponent implements OnInit {
  private projectService = inject(ProjectService);
  readonly authService = inject(AuthService);
  private fb = inject(FormBuilder);

  projects = signal<ProjectResponseDto[]>([]);
  isLoading = signal(false);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = signal(6);
  searchTerm = signal('');

  showModal = signal(false);
  isSubmitting = signal(false);

  createForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    description: ['']
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoading.set(true);
    this.projectService
      .getProjects({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        searchTerm: this.searchTerm()
      })
      .subscribe({
        next: (res) => {
          this.projects.set(res.items);
          this.totalCount.set(res.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }

  onSearch(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.loadProjects();
  }

  onPageChange(newPage: number): void {
    this.pageNumber.set(newPage);
    this.loadProjects();
  }

  openCreateModal(): void {
    this.createForm.reset();
    this.showModal.set(true);
  }

  closeCreateModal(): void {
    this.showModal.set(false);
  }

  submitCreate(): void {
    if (this.createForm.invalid) return;

    this.isSubmitting.set(true);
    const { name, description } = this.createForm.getRawValue();

    this.projectService
      .createProject({ name: name!, description: description || undefined })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.closeCreateModal();
          this.loadProjects();
        },
        error: () => this.isSubmitting.set(false)
      });
  }

  deleteProject(id: string): void {
    if (!confirm('Are you sure you want to delete this project?')) return;

    this.projectService.deleteProject(id).subscribe({
      next: () => this.loadProjects()
    });
  }
}