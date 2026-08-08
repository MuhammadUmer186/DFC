import { Routes } from '@angular/router';
import { LoginComponent } from './Pages/login/login.component';
import { DashboardComponent } from './Layout/dashboard/dashboard.component';
import { UserComponent } from './Pages/user/user.component';
import { RegisterComponent } from './Pages/register/register.component';
import { MenuComponent } from './Pages/menu/menu.component';
import { AddPurchasedItemsDataComponent } from './Pages/Stores/PurchaseditemAdd/add-purchased-items-data/add-purchased-items-data.component';
import { KitchenOutDataComponent } from './Pages/Stores/KitchenOut/Kitchen out/kitchenoutdata/kitchenoutdata.component';
import { MenuuComponent } from './Pages/menuu/menuu.component';
import { CreateMenuComponent } from './Pages/create-menu/create-menu.component';
import { UpdateMenuComponent } from './Pages/update-menu/update-menu.component';
import { DeleteMenuComponent } from './Pages/delete-menu/delete-menu.component';
import { CategoryListComponent } from './Pages/category/category-list/category-list.component';
import { CategoryCreateComponent } from './Pages/category/category-create/category-create.component';
import { CategoryUpdateComponent } from './Pages/category/category-update/category-update.component';
import { MenuListComponent } from './Pages/Menu-items/menu-list/menu-list/menu-list.component';
import { MenuUpdateComponent } from './Pages/Menu-items/menu-update/menu-update.component';
import { MenuCreateComponent } from './Pages/Menu-items/menu-create/menu-create.component';
import { VendorListComponent } from './Pages/vendor/vendor-list/vendor-list.component';
import { VendorUpdateComponent } from './Pages/vendor/vendor-update/vendor-update.component';
import { VendorCreateComponent } from './Pages/vendor/vendor-create/vendor-create.component';
import { RawItemListComponent } from './Pages/rawitem/rawitem-list/rawitem-list.component';
import { RawItemUpdateComponent } from './Pages/rawitem/rawitem-update/rawitem-update.component';
import { RawItemCreateComponent } from './Pages/rawitem/rawitem-create/rawitem-create.component';
import { PurchaseOrderListComponent } from './Pages/Purchaseorder/purchaseorder-list/purchaseorder-list.component';
import { KitchenOutListComponent } from './Pages/kitchenout/kitchenout-list/kitchenout-list.component';
import { KitchenOutUpdateComponent } from './Pages/kitchenout/kitchenout-update/kitchenout-update.component';
import { KitchenOutCreateComponent } from './Pages/kitchenout/kitchenout-create/kitchenout-create.component';
import { StockSummaryComponent } from './Pages/stock-summary/stock-summary.component';
import { PurchaseOrderMainCreateComponent } from './Pages/Purchaseorder/purchaseorder-maincreate/purchaseorder-maincreate.component';
import { KitchenOutMainCreateComponent } from './Pages/kitchenout/kitchenout-maincreate/kitchenout-maincreate.component';
import { MenuOrderComponent } from './Pages/menu-order/menu-order.component';
import { AuthGuard } from './guards/auth.guard';
import { AppLayoutComponent } from './Layout/applayout/applayout.component';
import { VendorAccountComponent } from './Pages/vendor-account/vendor-account.component';
import { OrdersHistoryComponent } from './Pages/order-history/order-history.component';
import { WasteManagementComponent } from './Pages/wastemanagement/wastemanagement.component';
import { WasteListComponent } from './Pages/waste-list/waste-list.component';
import { UtilityBillCreateComponent } from './Pages/utilitybill/utility-bill-create/utility-bill-create.component';
import { UtilityBillSearchComponent } from './Pages/utilitybill/utility-bill-search/utility-bill-search.component';
import { EmployeeListComponent } from './Pages/Employee/employee-list/employee-list';
import { EmployeeFormComponent } from './Pages/Employee/employee-form/employee-form';

import { DailySalaryReportComponent } from './Pages/Salary/daily-salary-report/daily-salary-report';
import { EmployeeEditComponent } from './Pages/Employee/employee-edit/employee-edit';
import { SalaryPayComponent } from './Pages/Salary/salary-pay/salary-pay';
import { QueueComponent } from './Pages/queue/queue';
import { ReportComponent } from './Pages/report1/report1';
import { MenuItemReportComponent } from './Pages/menu-item-report/menu-item-report';
import { MenuRecipeComponent } from './Pages/menu-recipe/menu-recipe';

export const routes: Routes = [
    { path: "", redirectTo: "login", pathMatch: "full" },
     {
        path:"",component:UserComponent,
        children:[
            {
                path:"login",component:LoginComponent
            },
            {
                path:"signup",component:RegisterComponent,canActivate: [AuthGuard],
                data: { roles: ['MainAdmin','Admin'] }
            }]},
            {
                path:"",component:AppLayoutComponent,children:[
            {
                path:"ORDERCREATE",component:MenuOrderComponent,canActivate: [AuthGuard],
                data: { roles: ['Cashier', 'Admin','MainAdmin','Waiter'] }
            },
            {
                path:"Queue",component:QueueComponent,canActivate: [AuthGuard],
                data: { roles: ['Cashier', 'Admin','MainAdmin'] }
            },
            {
                path:"VendorLedger",component:VendorAccountComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"stock-summary",component:StockSummaryComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"MainPO",component:PurchaseOrderMainCreateComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"MainKO",component:KitchenOutMainCreateComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"MainWM",component:WasteManagementComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"WM-list",component:WasteListComponent,canActivate: [AuthGuard],
                data: { roles: ['StoreKeeper', 'Admin','MainAdmin'] }
            },
            {
                path:"Orderhistory",component:OrdersHistoryComponent,canActivate: [AuthGuard],
                data: { roles: ['Cashier','Admin','MainAdmin'] }
            },
            {
                path:"UB-create",component:UtilityBillCreateComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"emp-list",component:EmployeeListComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"emp-create",component:EmployeeFormComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"emp-edit",component:EmployeeEditComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"salary-pay",component:SalaryPayComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },{
                path:"report",component:ReportComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"daily-salary",component:DailySalaryReportComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"UB-searchbill",component:UtilityBillSearchComponent,canActivate: [AuthGuard],
                data: { roles: ['Admin','MainAdmin'] }
            },
            {
            path:"dashboard",component:DashboardComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
            path:"menu-item-report",component:MenuItemReportComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
            path:"menu-recipe",component:MenuRecipeComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"menu",component:MenuComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin','Cashier','Waiter','Admin'] }
            },
            {
                path:"Po",component:AddPurchasedItemsDataComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"Ko",component:KitchenOutDataComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"menuu",component:MenuuComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"create-menu",component:CreateMenuComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"update-menu",component:UpdateMenuComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"Delete-menu",component:DeleteMenuComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"category-list",component:CategoryListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"category-update",component:CategoryUpdateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"category-create",component:CategoryCreateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"menu-list",component:MenuListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"menu-update",component:MenuUpdateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"menu-create",component:MenuCreateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"vendor-list",component:VendorListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"vendor-update",component:VendorUpdateComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"vendor-create",component:VendorCreateComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"rawitem-list",component:RawItemListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"rawitem-update",component:RawItemUpdateComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"rawitem-create",component:RawItemCreateComponent,canActivate: [AuthGuard],
            data: { roles: ['Admin','MainAdmin'] }
            },
            {
                path:"purchaseorder-list",component:PurchaseOrderListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin','Admin'] }
            },
            {
                path:"kitchenout-list",component:KitchenOutListComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin','Admin'] }
            },
            {
                path:"kitchenout-update",component:KitchenOutUpdateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            },
            {
                path:"kitchenout-create",component:KitchenOutCreateComponent,canActivate: [AuthGuard],
            data: { roles: ['MainAdmin'] }
            }]}
        ]

