# DecalXe Frontend

A modern React TypeScript application for managing car decal designs and orders.

## Features

- **Authentication**: Secure login/logout system
- **Dashboard**: Overview of orders, designs, and statistics
- **Order Management**: Create, view, edit, and delete orders
- **Design Management**: Manage car decal designs with AI-generated and manual options
- **Responsive Design**: Modern UI built with Tailwind CSS
- **Real-time Data**: Integration with DecalXe API

## Tech Stack

- **React 18** with TypeScript
- **React Router** for navigation
- **Tailwind CSS** for styling
- **Axios** for API communication
- **Lucide React** for icons
- **Context API** for state management

## Getting Started

### Prerequisites

- Node.js (v14 or higher)
- npm or yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Configure environment variables:
```bash
cp .env.example .env
```
Edit `.env` and set your API URL:
```
REACT_APP_API_URL=http://localhost:5000/api
```

3. Start the development server:
```bash
npm start
```

The application will open at `http://localhost:3000`.

### Building for Production

```bash
npm run build
```

## Project Structure

```
src/
├── components/          # Reusable UI components
│   └── Layout.tsx      # Main layout with navigation
├── contexts/           # React contexts
│   └── AuthContext.tsx # Authentication context
├── pages/              # Page components
│   ├── Login.tsx       # Login page
│   ├── Dashboard.tsx   # Dashboard page
│   ├── Orders.tsx      # Orders management
│   └── Designs.tsx     # Designs management
├── services/           # API services
│   └── api.ts          # API client
├── types/              # TypeScript type definitions
│   └── index.ts        # Main types
└── utils/              # Utility functions
```

## Available Scripts

- `npm start` - Start development server
- `npm run build` - Build for production
- `npm test` - Run tests
- `npm run eject` - Eject from Create React App

## Features Overview

### Authentication
- Secure login with JWT tokens
- Automatic token refresh
- Protected routes

### Dashboard
- Statistics overview
- Recent orders and designs
- Quick actions

### Order Management
- List all orders with filtering and search
- Create new orders
- View order details
- Update order status
- Delete orders

### Design Management
- Grid view of all designs
- Filter by status and type (AI/Manual)
- Upload and manage design files
- Approval workflow

## API Integration

The frontend communicates with the DecalXe API backend. Make sure the API is running and accessible at the configured URL.

## Environment Variables

- `REACT_APP_API_URL` - Backend API base URL

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is part of the DecalXe system.
