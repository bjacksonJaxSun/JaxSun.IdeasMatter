# AI-First Payroll & HR Frontend

Modern React application built with TypeScript, Vite, and Tailwind CSS.

## Features

- 🎨 **Modern UI/UX** - Clean, responsive design with Tailwind CSS
- 🔐 **Authentication** - Login/Signup with form validation
- ⚡ **Fast Development** - Vite for instant hot reload
- 🎭 **Animations** - Smooth transitions with Framer Motion
- 📱 **Responsive** - Mobile-first design approach
- 🛡️ **Type Safety** - Full TypeScript support

## Quick Start

```bash
cd frontend
npm install
npm run dev
```

The app will be available at http://localhost:4000

## Project Structure

```
src/
├── components/
│   ├── auth/           # Login/Signup forms
│   ├── common/         # Reusable components
│   └── layout/         # Layout components
├── contexts/           # React contexts
├── pages/              # Page components
├── services/           # API services
├── styles/             # Global styles
└── utils/              # Utility functions
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build

## Technologies Used

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool
- **Tailwind CSS** - Styling
- **React Router** - Routing
- **React Hook Form** - Form handling
- **Framer Motion** - Animations
- **Axios** - HTTP client
- **React Hot Toast** - Notifications

## Authentication

The app includes a complete authentication system with:

- Email/password login and signup
- Form validation
- Password strength requirements
- OAuth integration (Google, Facebook)
- JWT token management
- Protected routes

## API Integration

The frontend is configured to work with the FastAPI backend:

- Proxy setup for API calls
- Axios interceptors for token management
- Error handling and user feedback
- Type-safe API responses

## Deployment

Build the app for production:

```bash
npm run build
```

The `dist` folder contains the production-ready files.