# DecalXe - Car Decal Management System

A comprehensive car decal design and order management system built with .NET Core API and React TypeScript frontend.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js (v14+)
- PostgreSQL
- Git

### Development Setup

1. **Clone and Setup**
```bash
git clone <repository-url>
cd decalxe
```

2. **Backend Setup**
```bash
# Update connection string in appsettings.json
# Run database migrations
dotnet ef database update

# Start API
dotnet run --urls=http://localhost:5000
```

3. **Frontend Setup**
```bash
cd decal-xe-frontend
npm install
npm start
```

4. **Or use the development script**
```bash
./start-dev.sh
```

## 📋 Access Points

- **Frontend Application**: http://localhost:3000
- **API Backend**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger

## 🏗️ Project Structure

```
├── Controllers/              # API Controllers
├── Models/                   # Data Models
├── Services/                 # Business Logic
├── Data/                     # Database Context
├── DTOs/                     # Data Transfer Objects
├── decal-xe-frontend/        # React Frontend
│   ├── src/
│   │   ├── components/       # Reusable Components
│   │   ├── pages/           # Page Components
│   │   ├── services/        # API Services
│   │   ├── types/           # TypeScript Types
│   │   └── contexts/        # React Contexts
├── start-dev.sh             # Development Startup Script
└── PROJECT_SUMMARY.md       # Detailed Documentation
```

## ✨ Features

### 🔐 Authentication & Security
- JWT-based authentication
- Role-based access control
- Protected routes and endpoints

### 📊 Dashboard
- Real-time statistics
- Recent orders and designs overview
- Quick actions

### 📦 Order Management
- Complete order lifecycle management
- Customer vehicle tracking
- Status updates and priority levels
- Search and filtering

### 🎨 Design Management
- Design upload and approval workflow
- AI-generated vs manual design tracking
- Version control and designer assignment

### 👥 User Management
- Customer and employee profiles
- Role-based permissions
- Account management

## 🛠️ Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **JWT** - Authentication
- **Swagger** - API documentation

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **React Router** - Navigation
- **Axios** - HTTP client
- **Lucide React** - Icons

## 🚀 Deployment

### Backend
```bash
dotnet publish -c Release
# Deploy to your preferred cloud platform
```

### Frontend
```bash
cd decal-xe-frontend
npm run build
# Deploy build folder to static hosting
```

## �� API Documentation

The API is fully documented with Swagger/OpenAPI. Access the interactive documentation at:
`http://localhost:5000/swagger`

## 🧪 Testing

### Backend
```bash
dotnet test
```

### Frontend
```bash
cd decal-xe-frontend
npm test
```

## 🔧 Configuration

### Backend Configuration
Update `appsettings.json` with your database connection string and JWT settings.

### Frontend Configuration
Update `.env` in the frontend directory with your API URL:
```
REACT_APP_API_URL=http://localhost:5000/api
```

## 📚 Documentation

- [Project Summary](PROJECT_SUMMARY.md) - Comprehensive project documentation
- [API Endpoints](API_ENDPOINTS_SUMMARY.md) - API reference
- [Frontend README](decal-xe-frontend/README.md) - Frontend-specific documentation

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is part of the DecalXe system.

## 🆘 Support

For support and questions, please refer to the project documentation or create an issue in the repository.

---

**DecalXe** - Transform your vehicle with professional car decal designs! 🚗✨
