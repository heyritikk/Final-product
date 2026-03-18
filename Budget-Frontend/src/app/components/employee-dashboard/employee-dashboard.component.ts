import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Expense, ExpenseService } from '../../services/expense.service';
import { AuthService } from '../../services/auth.service';
import { BudgetService } from '../../services/budget.service';
import { ApiService, UserSummary } from '../../services/api.service';
import{NotificationService,AppNotification} from '../../services/notification.service';

interface NotificationItem {
notificationId: number;
type: string;
message: string;
readStatus: 'Read' | 'Unread';
createdAt: Date;
isLocal?: boolean;
}
@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-dashboard.component.html',
  styleUrls: ['./employee-dashboard.component.css']
})
export class EmployeeDashboardComponent implements OnInit {

  section: string = 'create-expense';
   isSidebarOpen = true;

  expenses: Expense[] = [];
  notifications: NotificationItem[] = [];
  unreadNotificationsCount: number = 0;
  budgets:any[] =[];
  managers: UserSummary[] = [];

  expenseForm!: FormGroup;
  editExpenseForm!: FormGroup;
  isLoadingExpenses = false;
  showEditModal = false;
  selectedExpense: Expense | null = null;

  loggedInEmail: string | null = null;

  constructor(
    private expenseService: ExpenseService,
    private budgetService:BudgetService,
    private authService: AuthService,
    private router: Router,
    private fb: FormBuilder,
    private apiService: ApiService,
    private notificationService: NotificationService
  ) {
    // DI: Angular injects these services (ExpenseService/BudgetService/AuthService/etc.)
    // These services wrap backend API calls (see src/app/services/*.service.ts).
  }

  ngOnInit(): void {
    this.loggedInEmail = localStorage.getItem('email');
    this.buildForms();
    this.loadExpenses();
    this.loadBudgets();
    this.loadManagers();
    this.setupBudgetManagerBinding();
    this.loadNotifications();
  }

  buildForms() {
    this.expenseForm = this.fb.group({
      title: ['', [Validators.maxLength(100)]],
      amount: [null, [Validators.required, Validators.min(1)]],
      budgetId: [null, [Validators.required]],
      description: ['', [Validators.maxLength(250)]],
      managerId: [null, [Validators.required]]
    });

    this.editExpenseForm = this.fb.group({
      // title: ['', [Validators.required, Validators.maxLength(100)]],
      amount: [null, [Validators.required, Validators.min(1)]],
      budgetId: [null, [Validators.required]],
      description: ['', [Validators.maxLength(250)]]
    });
  }

  get f() { return this.expenseForm.controls; }

  get displayName(): string {
    if (!this.loggedInEmail) {
      return 'Employee';
    }
    const [name] = this.loggedInEmail.split('@');
    return name || this.loggedInEmail;
  }

  setSection(name: string) {
this.section = name;
if (name === 'notifications') {
this.loadNotifications();
}
}

  get selectedBudgetCreatorId(): number | null {
 const selectedBudgetId = this.expenseForm?.get('budgetId')?.value;
 if (!selectedBudgetId) {
 return null;
 }
 const budget = this.budgets.find(b => Number(b.budgetId) === Number(selectedBudgetId));
 return budget?.createdByUserId ? Number(budget.createdByUserId) : null;
 }

  get filteredManagers(): UserSummary[] {
 const creatorId = this.selectedBudgetCreatorId;
 if (!creatorId) {
 return [];
 }
 return this.managers.filter(m => Number(m.userId) === creatorId);
 }
  loadExpenses() {
    this.isLoadingExpenses = true;
    // API call: GET /api/expense via ExpenseService
    this.expenseService.getAllExpenses().subscribe({
      next: (res) => {
        // After API success: update UI list and stop loader
        this.expenses = res;
        console.log("Expenses:", this.expenses[0]);
        this.isLoadingExpenses = false;
        //this.buildNotificationsFromExpenses();
      },
      error: () => {
        this.isLoadingExpenses = false;
      }
    });
  }

  createExpense() {
    if (this.expenseForm.invalid) {
      this.expenseForm.markAllAsTouched();
      return;
    }

    const raw = { ...this.expenseForm.getRawValue() };
    const dto = {
      description: String(raw.description ?? '').trim(),
      amount: Number(raw.amount),
      budgetId: Number(raw.budgetId),
      managerId: Number(raw.managerId)
    };

    if (!dto.managerId || Number.isNaN(dto.managerId)) {
      this.expenseForm.get('managerId')?.setErrors({ required: true });
      this.expenseForm.markAllAsTouched();
      return;
    }
    if (!dto.budgetId || Number.isNaN(dto.budgetId)) {
      this.expenseForm.get('budgetId')?.setErrors({ required: true });
      this.expenseForm.markAllAsTouched();
      return;
    }
    if (!dto.amount || Number.isNaN(dto.amount)) {
      this.expenseForm.get('amount')?.setErrors({ required: true });
      this.expenseForm.markAllAsTouched();
      return;
    }

    // API call: POST /api/expense/create (backend validates budget limit + role)
    this.expenseService.createExpense(dto).subscribe({
      next: () => {
        // After API success: reload list + reset form + navigate section
        this.loadExpenses();
        this.expenseForm.reset();
        this.section = 'view-expenses';
      },
      error: (err) => {
        // After API failure: show backend error message (BadRequest/Unauthorized/etc.)
        console.error('Create expense failed:', err);
        const msg =
          err?.error?.message ||
          err?.error?.title ||
          err?.message ||
          'Failed to create expense.';
        alert(msg);
      }
    });
  }

  openEditModal(expense: Expense) {
    this.selectedExpense = expense;
    this.editExpenseForm.patchValue({
     // title: expense.title,
      amount: expense.amount,
      budgetId: expense.budgetId,
      description: expense.description || ''
    });
    this.showEditModal = true;
  }

  closeEditModal() {
    this.showEditModal = false;
    this.selectedExpense = null;
  }

  updateExpense() {
    if (!this.selectedExpense || this.editExpenseForm.invalid) {
      this.editExpenseForm.markAllAsTouched();
      return;
    }

    const payload = this.editExpenseForm.value;

    this.expenseService.updateExpense(
      this.selectedExpense.id, payload).subscribe({
      next: () => {
        this.loadExpenses();
        this.closeEditModal();
      },
      error: () => {
        // optional: surface error
      }
    });
  }

  deleteExpense(id: number) {
    if (!confirm('Are you sure you want to delete this expense?')) {
      return;
    }

    this.expenseService.deleteExpense(id).subscribe({
      next: () => {
        this.expenses = this.expenses.filter(e => e.id !== id);
      
       // this.loadExpenses();
      }
    });
  }


  // buildNotificationsFromExpenses() {
  //   this.notifications = this.expenses
  //     .filter(e => e.status === 'Approved' || e.status === 'Rejected')
  //     .map(e => ({
  //       message: e.status === 'Approved'
  //         ? `Your expense "${e.description}" was approved.`
  //         : `Your expense "${e.description}" was rejected.`,
  //       status: e.status as 'Approved' | 'Rejected',
  //       createdAt: new Date()
  //     }));
  // }
//   buildNotificationsFromExpenses() {
//  this.notifications = this.expenses
//  .filter(e => e.status === 'Approved' || e.status === 'Rejected')
//  .map(e => ({
//  message: e.status === 'Approved'
//  ? `Your expense "${e.description}" was approved.`
//  : `Your expense "${e.description}" was rejected.`,
//  status: e.status as 'Approved' | 'Rejected',
//  createdAt: new Date()
//  }));
//  }
private normalizeReadStatus(value: string | number | null | undefined): 'Read' | 'Unread' {
if (value === 1 || String(value).toLowerCase() === 'read') {
return 'Read';
}
return 'Unread';
}
 
private mapNotificationType(type: string | number): string {
if (typeof type === 'string') {
return type;
}
if (type === 0) return 'ExpenseApproval';
if (type === 1) return 'ExpenseRejected';
if (type === 2) return 'ExpensePending';
return 'Notification';
}
 
loadNotifications() {
// API call: GET /api/notifications
this.notificationService.getNotifications().subscribe({
next: (res) => {
const list = res?.data ?? [];
this.notifications = list.map((n: AppNotification) => ({
notificationId: n.notificationId,
type: this.mapNotificationType(n.type),
message: n.message,
readStatus: this.normalizeReadStatus(n.status),
createdAt: n.createdDate ? new Date(n.createdDate) : new Date()
}));
this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
},
error: () => {
this.notifications = [];
this.unreadNotificationsCount = 0;
}
});
}
 
markNotificationAsRead(notification: NotificationItem) {
if (notification.readStatus === 'Read') {
return;
}
 
if (notification.isLocal || !notification.notificationId) {
notification.readStatus = 'Read';
this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
return;
}
 
this.notificationService.markAsRead(notification.notificationId).subscribe({
next: () => {
notification.readStatus = 'Read';
this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
},
error: () => {
// keep previous status on failure
}
});
}

 setupBudgetManagerBinding() {
 this.expenseForm.get('budgetId')?.valueChanges.subscribe((budgetId) => {
 this.syncManagerWithSelectedBudget(budgetId);
 });
 }
  syncManagerWithSelectedBudget(budgetId: number | string | null) {
 const selected = this.budgets.find(b => Number(b.budgetId) === Number(budgetId));
 const creatorId = selected?.createdByUserId ? Number(selected.createdByUserId) : null;
 const managerControl = this.expenseForm.get('managerId');

 if (!managerControl) {
 return;
 }
 if (!creatorId) {
 managerControl.setValue(null, { emitEvent: false });
 return;
 }

 if (Number(managerControl.value) !== creatorId) {
 managerControl.setValue(creatorId, { emitEvent: false });
 }
 }



  loadManagers() {
    this.apiService.getManagers().subscribe({
      next: (res) => {
        this.managers = res || [];
      },
      error: () => {
        this.managers = [];
      }
    });
  }

   loadBudgets(){
    
    this.budgetService.getAllBudgets().subscribe
       ((res:any) => {
        console.log("Full Response",res);
        this.budgets = res.data;
        console.log("Budget Array:",this.budgets);
       
      });
      
  }

   toggleSidebar() {
 this.isSidebarOpen = !this.isSidebarOpen;
 }

  

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
