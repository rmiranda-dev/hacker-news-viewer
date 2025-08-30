import { Component, EventEmitter, Output, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate } from '@angular/animations';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="search-container" [@slideIn]>
      <mat-form-field appearance="fill" class="search-field" hideRequiredMarker>
        <input 
          matInput
          [formControl]="searchControl"
          placeholder="Search Hacker News stories..."
          type="text"
          class="search-input">
        <mat-icon matPrefix class="search-icon">search</mat-icon>
        <button 
          *ngIf="searchControl.value"
          matSuffix
          mat-icon-button
          aria-label="Clear search"
          (click)="clearSearch()"
          type="button"
          class="clear-button">
          <mat-icon>close</mat-icon>
        </button>
      </mat-form-field>
    </div>
  `,
  styles: [`
    .search-container {
      max-width: 600px;
      margin: 24px auto;
      padding: 0 16px;
    }
    
    .search-field {
      width: 100%;
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.08);
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      overflow: hidden;
    }
    
    .search-field:focus-within {
      box-shadow: 0 8px 30px rgba(63, 81, 181, 0.15);
      transform: translateY(-2px);
    }
    
    .search-input {
      font-size: 16px;
      color: #333;
      font-weight: 400;
    }
    
    .search-icon {
      color: #3f51b5;
      margin-right: 8px;
      opacity: 0.8;
    }
    
    .clear-button {
      color: #666;
      transition: all 0.2s ease;
      margin-right: 4px;
    }
    
    .clear-button:hover {
      background-color: rgba(63, 81, 181, 0.1);
      color: #3f51b5;
    }
    
    /* Remove Material Design default styling */
    .search-field ::ng-deep .mat-mdc-form-field-subscript-wrapper {
      display: none;
    }
    
    .search-field ::ng-deep .mat-mdc-form-field-bottom-align::before {
      display: none;
    }
    
    .search-field ::ng-deep .mat-mdc-form-field-infix {
      padding: 16px 0;
      border: none;
      min-height: auto;
    }
    
    .search-field ::ng-deep .mdc-text-field {
      background-color: transparent;
      border-radius: 12px;
    }
    
    .search-field ::ng-deep .mdc-text-field--filled {
      background-color: white;
    }
    
    .search-field ::ng-deep .mdc-text-field--filled::before,
    .search-field ::ng-deep .mdc-text-field--filled::after {
      display: none;
    }
    
    .search-field ::ng-deep .mdc-line-ripple {
      display: none;
    }
    
    .search-field ::ng-deep .mat-mdc-form-field-focus-overlay {
      display: none;
    }
    
    /* Hide floating label since we're using placeholder */
    .search-field ::ng-deep .mat-mdc-floating-label {
      display: none;
    }
    
    .search-field ::ng-deep .mat-mdc-form-field-label-wrapper {
      display: none;
    }
    
    /* Style the placeholder */
    .search-field ::ng-deep input::placeholder {
      color: #999;
      font-size: 16px;
      font-weight: 400;
    }
    
    @media (max-width: 600px) {
      .search-container {
        padding: 0 12px;
        margin: 16px auto;
      }
      
      .search-field {
        border-radius: 8px;
      }
    }
  `],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-20px)' }),
        animate('0.4s cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class SearchBoxComponent implements OnInit, OnDestroy {
  @Output() searchChange = new EventEmitter<string>();
  
  searchControl = new FormControl('');
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Set up debounced search
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300), // Wait 300ms after user stops typing
        distinctUntilChanged(), // Only emit if value actually changed
        takeUntil(this.destroy$) // Clean up subscription on destroy
      )
      .subscribe(searchTerm => {
        this.searchChange.emit(searchTerm || '');
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    // Note: The valueChanges subscription will automatically emit the cleared value
  }
}
