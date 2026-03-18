
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Budget, BudgetService } from '../../services/budget.service';
import { Expense, ExpenseService } from '../../services/expense.service';
import { NotificationService,AppNotification } from '../../services/notification.service';

interface NotificationItem {
 notificationId: number;
 readStatus: 'Read' | 'Unread';
 message: string;
 type: 'expense' | 'budget';
 createdAt: Date;
 isLocal?: boolean;
}

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './manager-dashboard.component.html',
  styleUrls: ['./manager-dashboard.component.css']
})
export class ManagerDashboardComponent implements OnInit {

  section: string = 'create-budget';
   isSidebarOpen = true;

  budgets: Budget[] = [];
  expenses: Expense[] = [];
  notifications: NotificationItem[] = [];
  unreadNotificationsCount = 0;


  pendingExpensesCount = 0;

  loggedInEmail: string | null = null;

  departments = [
    { departmentId: 1, departmentName: 'HR' },
    { departmentId: 2, departmentName: 'Finance' },
    { departmentId: 3, departmentName: 'IT' }
  ];

  budgetForm!: FormGroup;
  editBudgetForm!: FormGroup;
  showEditModal = false;
  selectedBudget: Budget | null = null;

  isLoadingBudgets = false;
  isLoadingExpenses = false;

  approvalFilter: 'Pending' | 'Approved' | 'Rejected' | 'All' = 'Pending';

  constructor(
    private budgetService: BudgetService,
    private expenseService: ExpenseService,
    private router: Router,
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService
  ){
    // DI: Angular injects services used to call backend APIs.
    // BudgetService -> budgets endpoints, ExpenseService -> expense endpoints, NotificationService -> notifications endpoints.
  }

  ngOnInit(){
    this.loggedInEmail = this.authService.getEmail();
    this.buildForms();
    this.loadBudgets();
    this.loadExpenses();
    this.loadNotifications();
   // this.pendingExpensesCount = this.expenses.filter(e => e.status === 0).length;
  }

  buildForms() {
    this.budgetForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      amountAllocated: [null, [Validators.required, Validators.min(1)]],
      departmentId: [null, [Validators.required]]
    });

    this.editBudgetForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      amountAllocated: [null, [Validators.required, Validators.min(1)]],
      departmentId: [null, [Validators.required]]
    });
  }

  get f() { return this.budgetForm.controls; }


  // get displayName(): string {
 
  // const name = localStorage.getItem('name');
 

  get displayName(): string {
    if (!this.loggedInEmail) {
      return 'Manager';
    }
    const [name] = this.loggedInEmail.split('@');
    return name || this.loggedInEmail;
  }

  
  get filteredExpenses(): Expense[] {
    if (this.approvalFilter === 'All') {
      return this.expenses;
    }
    return this.expenses.filter(e => e.status === Number(this.approvalFilter));
  }

  setSection(name:string){
    this.section = name;
    if (name === 'view-budgets') {
      this.loadBudgets();
    }
    if (name === 'expense-approvals') {
      this.loadExpenses();

     } 
    if (name === 'notifications') {
     this.loadNotifications();
    }
  }
  toggleSidebar() {
 this.isSidebarOpen = !this.isSidebarOpen;
 }

  setApprovalFilter(filter: 'Pending' | 'Approved' | 'Rejected' | 'All') {
    this.approvalFilter = filter;
  }

  loadBudgets(){
    this.isLoadingBudgets = true;
    // API call: GET budgets via BudgetService
    this.budgetService.getAllBudgets().subscribe({
      next: (res : any) => {
        const currentUserId=this.authService.getUserId();
        console.log("CurrentUserId:",currentUserId);
        console.log("First manager",res.data[0]);
        this.budgets = (res.data||[]).filter((b:any) => 
       String(currentUserId) === String
         (b.createdByUserId));      
        
        console.log("DATA:",res);
        
        this.isLoadingBudgets = false;
      },
      error: () => {
        this.isLoadingBudgets = false;
      }
    });
  }

  
  loadExpenses() {
  this.isLoadingExpenses = true;
 
  // API call: GET /api/expense (manager then filters client-side by managerId/email)
  this.expenseService.getAllExpenses().subscribe({
    next: (res: any) => {
 
      console.log("FULL RESPONSE:", res);
 
      const currentUserId = this.authService.getUserId();
      const currentEmail = this.authService.getEmail();
     
      const expenseArray = res;
      console.log("expense",expenseArray);
 
        
        this.expenses = (res||[]).filter((e:any) => {
          console.log("Expense ManagerId:", e.managerId);
          if (e.managerId && currentUserId) {

            return String(e.managerId) === String(currentUserId);
          }
          if (e.managerEmail && currentEmail) {
            return e.managerEmail.toLowerCase() === currentEmail.toLowerCase();
          }
          return e.status === 0;
        });
    

      this.pendingExpensesCount =
        this.expenses.filter(e => e.status === 0).length;
 
      this.isLoadingExpenses = false;
    },
    error: () => {
      this.isLoadingExpenses = false;
    }
  });
}
 

  createBudget(){
    if (this.budgetForm.invalid) {
      this.budgetForm.markAllAsTouched();
      return;
    }

    const payload = this.budgetForm.value;

    // API call: POST create budget
    this.budgetService.createBudget(payload).subscribe({
      next: () => {
        // After API success: add local notification + reload budgets + reset form
        this.pushNotification('Budget created successfully.', 'budget');
        this.loadBudgets();
        this.budgetForm.reset();
        this.section = 'view-budgets';
      },
      error: () => {
        this.pushNotification('Failed to create budget.', 'budget');
      }
    });
  }

  openEditModal(budget: Budget) {
    this.selectedBudget = budget;
    this.editBudgetForm.setValue({
      title: budget.title,
      amountAllocated: budget.amountAllocated,
      departmentId: budget.departmentId
    });
    this.showEditModal = true;
  }

  closeEditModal() {
    this.showEditModal = false;
    this.selectedBudget = null;
  }

  updateBudget() {
    if (!this.selectedBudget || this.editBudgetForm.invalid) {
      this.editBudgetForm.markAllAsTouched();
      return;
    }

    const payload = this.editBudgetForm.value;

    this.budgetService.updateBudget(this.selectedBudget.budgetId, payload).subscribe({
      next: () => {
        this.pushNotification('Budget updated successfully.', 'budget');
        this.loadBudgets();
        this.closeEditModal();
      },
      error: () => {
        this.pushNotification('Failed to update budget.', 'budget');
      }
    });
  }

  deleteBudget(budgetId:number){
    if (!confirm('Are you sure you want to delete this budget?')) {
      return;
    }
    this.budgetService.deleteBudget(budgetId).subscribe(()=>{
      this.pushNotification('Budget deleted.', 'budget');
      this.loadBudgets();
    });
  }

  approveExpense(expense: Expense) {
    if (!this.canManageExpense(expense)) {
      return;
    }
    console.log("Expense",expense);
    console.log("ExpenseId",expense.id);
    // API call: PATCH /api/expense/approve-reject/{id}
    this.expenseService.approveOrRejectExpense(expense.id, 'Approve').subscribe({
      next: () => {
        // After API success: local UI update + local notification
        this.pushNotification(`Expense "${expense.description}" approved.`, 'expense');
        this.expenses = this.expenses.map(e =>
          e.id === expense.id ? { ...e, status: 'Approved' } : e
        );
        this.pendingExpensesCount = this.expenses.filter(e => e.status === "Pending").length;
      },
      error: () => {
        this.pushNotification(`Failed to approve expense "${expense.description}".`, 'expense');
      }
    });
  }

  rejectExpense(expense: Expense) {
    if (!this.canManageExpense(expense)) {
      return;
    }
    // API call: PATCH /api/expense/approve-reject/{id}
    this.expenseService.approveOrRejectExpense(expense.id, 'Reject').subscribe({
      next: () => {
        // After API success: local UI update + local notification
        this.pushNotification(`Expense "${expense.description}" rejected.`, 'expense');
        this.expenses = this.expenses.map(e =>
          e.id === expense.id ? { ...e, status: 'Rejected' } : e
        );
        this.pendingExpensesCount = this.expenses.filter(e => e.status === "Pending").length;
      },
      error: () => {
        this.pushNotification(`Failed to reject expense "${expense.description}".`, 'expense');
      }
    });
  }

  pushNotification(message: string, type: 'expense' | 'budget') {
    this.notifications.unshift({
      notificationId: 0,
      readStatus: 'Unread',
      message,
      type,
      createdAt: new Date(),
      isLocal: true
    });
     this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
  }

  private normalizeReadStatus(value: string | number | null | undefined): 'Read' | 'Unread' {
 if (value === 1 || String(value).toLowerCase() === 'read') {
 return 'Read';
 }
 return 'Unread';
 }

  private mapNotificationType(type: string | number): 'expense' | 'budget' {
 if (typeof type === 'string') {
 const lowered = type.toLowerCase();
 if (lowered.includes('budget')) return 'budget';
 return 'expense';
 }
 return type === 2 ? 'expense' : 'expense';
 }
  canManageExpense(expense: Expense): boolean {
    const currentEmail = this.authService.getEmail();
    const currentUserId = this.authService.getUserId();

    if (expense.managerId && currentUserId) {
      return String(expense.managerId) === String(currentUserId);
    }
    if (expense.managerEmail && currentEmail) {
      return expense.managerEmail.toLowerCase() === currentEmail.toLowerCase();
    }
    return false;
  }

  getDepartmentName(id:number):string{
    const dept = this.departments.find(d => d.departmentId === id);
    return dept ? dept.departmentName : 'Unknown';
  }
  loadNotifications() {
 // API call: GET /api/notifications
 this.notificationService.getNotifications().subscribe({
 next: (res) => {
 const serverNotifications = (res?.data ?? []).map((n: AppNotification) => ({
 notificationId: n.notificationId,
 readStatus: this.normalizeReadStatus(n.status),
 message: n.message,
 type: this.mapNotificationType(n.type),
 createdAt: n.createdDate ? new Date(n.createdDate) : new Date(),
 isLocal: false
 }));
 const localNotifications = this.notifications.filter(n => n.isLocal || !n.notificationId);
 this.notifications = [...localNotifications, ...serverNotifications];
 this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
 },
 error: () => {
 this.unreadNotificationsCount = this.notifications.filter(n => n.readStatus === 'Unread').length;
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

 // API call: PATCH /api/notifications/read/{notificationId}
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

  // loadNotifications() {
  //   const userId = this.authService.getUserId();
  //   if (!userId) {
  //     console.warn('No user ID found for loading notifications.');
  //     return;
  //   } 
  //   this.notificationService.getNotifications(Number(userId)).subscribe({next:(res:any) => {
  //     console.log("Notifications:", res);
  //     this.notifications = res.data;
  //   }, error: () => {
  //     console.error("Notification error");
  //   }
  //   });}
  
  logout(){
    this.authService.logout();
    this.router.navigate(['/login']);
  }

}