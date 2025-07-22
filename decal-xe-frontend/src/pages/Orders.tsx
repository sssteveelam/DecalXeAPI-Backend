import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiService } from '../services/api';
import { Order } from '../types';
import { 
  Package, 
  Plus, 
  Search, 
  Filter,
  Eye,
  Edit,
  Trash2,
  Calendar
} from 'lucide-react';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [sortBy, setSortBy] = useState('orderDate');

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      const data = await apiService.getOrders();
      setOrders(data);
    } catch (error) {
      console.error('Error fetching orders:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteOrder = async (orderId: string) => {
    if (window.confirm('Are you sure you want to delete this order?')) {
      try {
        await apiService.deleteOrder(orderId);
        setOrders(orders.filter(order => order.orderID !== orderId));
      } catch (error) {
        console.error('Error deleting order:', error);
        alert('Failed to delete order. Please try again.');
      }
    }
  };

  const getOrderStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'text-green-800 bg-green-100';
      case 'pending':
        return 'text-yellow-800 bg-yellow-100';
      case 'cancelled':
        return 'text-red-800 bg-red-100';
      case 'in progress':
        return 'text-blue-800 bg-blue-100';
      default:
        return 'text-gray-800 bg-gray-100';
    }
  };

  const getPriorityColor = (priority?: string) => {
    switch (priority?.toLowerCase()) {
      case 'high':
        return 'text-red-800 bg-red-100';
      case 'medium':
        return 'text-yellow-800 bg-yellow-100';
      case 'low':
        return 'text-green-800 bg-green-100';
      default:
        return 'text-gray-800 bg-gray-100';
    }
  };

  const filteredOrders = orders
    .filter(order => {
      const matchesSearch = order.customer?.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           order.orderID.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === '' || order.orderStatus === statusFilter;
      return matchesSearch && matchesStatus;
    })
    .sort((a, b) => {
      switch (sortBy) {
        case 'orderDate':
          return new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime();
        case 'totalAmount':
          return b.totalAmount - a.totalAmount;
        case 'status':
          return a.orderStatus.localeCompare(b.orderStatus);
        default:
          return 0;
      }
    });

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Orders</h1>
          <p className="text-gray-600">Manage your car decal orders</p>
        </div>
        <Link
          to="/orders/new"
          className="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center space-x-2"
        >
          <Plus className="h-4 w-4" />
          <span>New Order</span>
        </Link>
      </div>

      {/* Filters */}
      <div className="bg-white p-4 rounded-lg shadow">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search orders..."
              className="pl-10 w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          
          <select
            className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="New">New</option>
            <option value="Pending">Pending</option>
            <option value="In Progress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>

          <select
            className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="orderDate">Sort by Date</option>
            <option value="totalAmount">Sort by Amount</option>
            <option value="status">Sort by Status</option>
          </select>

          <div className="flex items-center text-sm text-gray-600">
            <Package className="h-4 w-4 mr-2" />
            {filteredOrders.length} orders found
          </div>
        </div>
      </div>

      {/* Orders Table */}
      <div className="bg-white shadow overflow-hidden sm:rounded-md">
        <ul className="divide-y divide-gray-200">
          {filteredOrders.length === 0 ? (
            <li className="p-6 text-center text-gray-500">
              No orders found matching your criteria
            </li>
          ) : (
            filteredOrders.map((order) => (
              <li key={order.orderID} className="hover:bg-gray-50">
                <div className="px-6 py-4">
                  <div className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center space-x-3">
                        <div className="flex-shrink-0">
                          <Package className="h-8 w-8 text-gray-400" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">
                            Order #{order.orderID.slice(-8)}
                          </p>
                          <p className="text-sm text-gray-600">
                            {order.customer?.fullName || 'Unknown Customer'}
                          </p>
                          <div className="flex items-center space-x-2 mt-1">
                            <Calendar className="h-3 w-3 text-gray-400" />
                            <span className="text-xs text-gray-500">
                              {new Date(order.orderDate).toLocaleDateString()}
                            </span>
                            {order.isCustomDecal && (
                              <span className="text-xs bg-purple-100 text-purple-800 px-2 py-1 rounded-full">
                                Custom
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    </div>
                    
                    <div className="flex items-center space-x-4">
                      <div className="text-right">
                        <p className="text-sm font-medium text-gray-900">
                          ${order.totalAmount.toFixed(2)}
                        </p>
                        <div className="flex items-center space-x-2">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${getOrderStatusColor(order.orderStatus)}`}>
                            {order.orderStatus}
                          </span>
                          {order.priority && (
                            <span className={`px-2 py-1 text-xs font-medium rounded-full ${getPriorityColor(order.priority)}`}>
                              {order.priority}
                            </span>
                          )}
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-2">
                        <Link
                          to={`/orders/${order.orderID}`}
                          className="text-primary-600 hover:text-primary-700 p-1"
                          title="View Order"
                        >
                          <Eye className="h-4 w-4" />
                        </Link>
                        <Link
                          to={`/orders/${order.orderID}/edit`}
                          className="text-gray-600 hover:text-gray-700 p-1"
                          title="Edit Order"
                        >
                          <Edit className="h-4 w-4" />
                        </Link>
                        <button
                          onClick={() => handleDeleteOrder(order.orderID)}
                          className="text-red-600 hover:text-red-700 p-1"
                          title="Delete Order"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </li>
            ))
          )}
        </ul>
      </div>
    </div>
  );
};

export default Orders;
