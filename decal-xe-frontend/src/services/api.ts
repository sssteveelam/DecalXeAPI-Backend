import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { 
  LoginRequest, 
  LoginResponse, 
  Account, 
  Order, 
  Design, 
  Store,
  Customer,
  CustomerVehicle,
  ApiResponse 
} from '../types';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: process.env.REACT_APP_API_URL || 'https://localhost:5000/api',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('token');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response: AxiosResponse<LoginResponse> = await this.api.post('/auth/login', credentials);
    return response.data;
  }

  async register(userData: Partial<Account>): Promise<Account> {
    const response: AxiosResponse<Account> = await this.api.post('/auth/register', userData);
    return response.data;
  }

  // Orders endpoints
  async getOrders(): Promise<Order[]> {
    const response: AxiosResponse<Order[]> = await this.api.get('/orders');
    return response.data;
  }

  async getOrder(orderId: string): Promise<Order> {
    const response: AxiosResponse<Order> = await this.api.get(`/orders/${orderId}`);
    return response.data;
  }

  async createOrder(orderData: Partial<Order>): Promise<Order> {
    const response: AxiosResponse<Order> = await this.api.post('/orders', orderData);
    return response.data;
  }

  async updateOrder(orderId: string, orderData: Partial<Order>): Promise<Order> {
    const response: AxiosResponse<Order> = await this.api.put(`/orders/${orderId}`, orderData);
    return response.data;
  }

  async deleteOrder(orderId: string): Promise<void> {
    await this.api.delete(`/orders/${orderId}`);
  }

  // Designs endpoints
  async getDesigns(): Promise<Design[]> {
    const response: AxiosResponse<Design[]> = await this.api.get('/designs');
    return response.data;
  }

  async getDesign(designId: string): Promise<Design> {
    const response: AxiosResponse<Design> = await this.api.get(`/designs/${designId}`);
    return response.data;
  }

  async createDesign(designData: Partial<Design>): Promise<Design> {
    const response: AxiosResponse<Design> = await this.api.post('/designs', designData);
    return response.data;
  }

  async updateDesign(designId: string, designData: Partial<Design>): Promise<Design> {
    const response: AxiosResponse<Design> = await this.api.put(`/designs/${designId}`, designData);
    return response.data;
  }

  async deleteDesign(designId: string): Promise<void> {
    await this.api.delete(`/designs/${designId}`);
  }

  // Stores endpoints
  async getStores(): Promise<Store[]> {
    const response: AxiosResponse<Store[]> = await this.api.get('/stores');
    return response.data;
  }

  // Customers endpoints
  async getCustomers(): Promise<Customer[]> {
    const response: AxiosResponse<Customer[]> = await this.api.get('/customers');
    return response.data;
  }

  async createCustomer(customerData: Partial<Customer>): Promise<Customer> {
    const response: AxiosResponse<Customer> = await this.api.post('/customers', customerData);
    return response.data;
  }

  // Customer Vehicles endpoints
  async getCustomerVehicles(customerId: string): Promise<CustomerVehicle[]> {
    const response: AxiosResponse<CustomerVehicle[]> = await this.api.get(`/customers/${customerId}/vehicles`);
    return response.data;
  }

  async createCustomerVehicle(vehicleData: Partial<CustomerVehicle>): Promise<CustomerVehicle> {
    const response: AxiosResponse<CustomerVehicle> = await this.api.post('/vehicles', vehicleData);
    return response.data;
  }
}

export const apiService = new ApiService();
export default apiService;
