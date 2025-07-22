import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiService } from '../services/api';
import { Design } from '../types';
import { 
  Palette, 
  Plus, 
  Search, 
  Eye,
  Edit,
  Trash2,
  Download,
  Sparkles
} from 'lucide-react';

const Designs: React.FC = () => {
  const [designs, setDesigns] = useState<Design[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');

  useEffect(() => {
    fetchDesigns();
  }, []);

  const fetchDesigns = async () => {
    try {
      const data = await apiService.getDesigns();
      setDesigns(data);
    } catch (error) {
      console.error('Error fetching designs:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteDesign = async (designId: string) => {
    if (window.confirm('Are you sure you want to delete this design?')) {
      try {
        await apiService.deleteDesign(designId);
        setDesigns(designs.filter(design => design.designID !== designId));
      } catch (error) {
        console.error('Error deleting design:', error);
        alert('Failed to delete design. Please try again.');
      }
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
        return 'text-green-800 bg-green-100';
      case 'pending':
        return 'text-yellow-800 bg-yellow-100';
      case 'rejected':
        return 'text-red-800 bg-red-100';
      case 'in review':
        return 'text-blue-800 bg-blue-100';
      default:
        return 'text-gray-800 bg-gray-100';
    }
  };

  const filteredDesigns = designs
    .filter(design => {
      const matchesSearch = design.designer?.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           design.designID.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           design.version.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === '' || design.approvalStatus === statusFilter;
      const matchesType = typeFilter === '' || 
                         (typeFilter === 'ai' && design.isAIGenerated) ||
                         (typeFilter === 'manual' && !design.isAIGenerated);
      return matchesSearch && matchesStatus && matchesType;
    })
    .sort((a, b) => parseFloat(b.version) - parseFloat(a.version));

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
          <h1 className="text-2xl font-bold text-gray-900">Designs</h1>
          <p className="text-gray-600">Manage your car decal designs</p>
        </div>
        <Link
          to="/designs/new"
          className="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center space-x-2"
        >
          <Plus className="h-4 w-4" />
          <span>New Design</span>
        </Link>
      </div>

      {/* Filters */}
      <div className="bg-white p-4 rounded-lg shadow">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search designs..."
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
            <option value="Pending">Pending</option>
            <option value="In Review">In Review</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>

          <select
            className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
          >
            <option value="">All Types</option>
            <option value="ai">AI Generated</option>
            <option value="manual">Manual Design</option>
          </select>

          <div className="flex items-center text-sm text-gray-600">
            <Palette className="h-4 w-4 mr-2" />
            {filteredDesigns.length} designs found
          </div>
        </div>
      </div>

      {/* Designs Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredDesigns.length === 0 ? (
          <div className="col-span-full text-center py-12 text-gray-500">
            No designs found matching your criteria
          </div>
        ) : (
          filteredDesigns.map((design) => (
            <div key={design.designID} className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow">
              {/* Design Preview */}
              <div className="aspect-w-16 aspect-h-9 bg-gray-200">
                {design.designURL ? (
                  <img
                    src={design.designURL}
                    alt={`Design v${design.version}`}
                    className="w-full h-48 object-cover"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2Y3ZjdmNyIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LXNpemU9IjE4IiBmaWxsPSIjOTk5IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+RGVzaWduIFByZXZpZXc8L3RleHQ+PC9zdmc+';
                    }}
                  />
                ) : (
                  <div className="w-full h-48 bg-gray-100 flex items-center justify-center">
                    <Palette className="h-12 w-12 text-gray-400" />
                  </div>
                )}
              </div>

              {/* Design Info */}
              <div className="p-4">
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-lg font-medium text-gray-900">
                    Version {design.version}
                  </h3>
                  <div className="flex items-center space-x-2">
                    {design.isAIGenerated && (
                      <div title="AI Generated">
                        <Sparkles className="h-4 w-4 text-purple-500" />
                      </div>
                    )}
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(design.approvalStatus)}`}>
                      {design.approvalStatus}
                    </span>
                  </div>
                </div>

                <p className="text-sm text-gray-600 mb-2">
                  Designer: {design.designer?.fullName || 'Unknown'}
                </p>

                {design.isAIGenerated && design.aiModelUsed && (
                  <p className="text-xs text-purple-600 mb-2">
                    AI Model: {design.aiModelUsed}
                  </p>
                )}

                <div className="flex items-center justify-between mb-4">
                  <span className="text-lg font-bold text-gray-900">
                    ${design.designPrice.toFixed(2)}
                  </span>
                </div>

                {/* Actions */}
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-2">
                    <Link
                      to={`/designs/${design.designID}`}
                      className="text-primary-600 hover:text-primary-700 p-1"
                      title="View Design"
                    >
                      <Eye className="h-4 w-4" />
                    </Link>
                    <Link
                      to={`/designs/${design.designID}/edit`}
                      className="text-gray-600 hover:text-gray-700 p-1"
                      title="Edit Design"
                    >
                      <Edit className="h-4 w-4" />
                    </Link>
                    <button
                      onClick={() => handleDeleteDesign(design.designID)}
                      className="text-red-600 hover:text-red-700 p-1"
                      title="Delete Design"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                  
                  {design.designURL && (
                    <a
                      href={design.designURL}
                      download
                      className="text-blue-600 hover:text-blue-700 p-1"
                      title="Download Design"
                    >
                      <Download className="h-4 w-4" />
                    </a>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Designs;
