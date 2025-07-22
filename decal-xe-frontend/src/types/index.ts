// API Types based on the backend models
export interface Account {
  accountID: string;
  username: string;
  email?: string;
  passwordHash: string;
  isActive: boolean;
  roleID: string;
  role?: Role;
}

export interface Role {
  roleID: string;
  roleName: string;
}

export interface Design {
  designID: string;
  designURL: string;
  designerID?: string;
  designer?: Employee;
  version: string;
  approvalStatus: string;
  isAIGenerated: boolean;
  aiModelUsed?: string;
  designPrice: number;
}

export interface Order {
  orderID: string;
  customerID: string;
  customer?: Customer;
  orderDate: string;
  totalAmount: number;
  orderStatus: string;
  assignedEmployeeID?: string;
  assignedEmployee?: Employee;
  vehicleID?: string;
  customerVehicle?: CustomerVehicle;
  expectedArrivalTime?: string;
  currentStage: string;
  priority?: string;
  isCustomDecal: boolean;
}

export interface Customer {
  customerID: string;
  fullName: string;
  phoneNumber: string;
  address?: string;
  accountID: string;
  account?: Account;
}

export interface Employee {
  employeeID: string;
  fullName: string;
  phoneNumber: string;
  position: string;
  accountID: string;
  account?: Account;
}

export interface CustomerVehicle {
  vehicleID: string;
  customerID: string;
  vehicleType: string;
  licensePlate: string;
  color: string;
  model: string;
  year: number;
}

export interface Store {
  storeID: string;
  storeName: string;
  address: string;
  phoneNumber: string;
  email?: string;
  isActive: boolean;
}

// Login/Auth types
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: Account;
}

// API Response wrapper
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}
