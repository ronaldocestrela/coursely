import { createBrowserRouter } from 'react-router-dom'

import { RequireAuth } from '@/components/RequireAuth'
import { MainLayout } from '@/layouts/MainLayout'
import { CreateCoursePage } from '@/pages/CreateCoursePage'
import { DashboardPage } from '@/pages/DashboardPage'
import { ForgotPasswordPage } from '@/pages/ForgotPasswordPage'
import { HomePage } from '@/pages/HomePage'
import { LoginPage } from '@/pages/LoginPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { ResetPasswordPage } from '@/pages/ResetPasswordPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'cadastro', element: <RegisterPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'esqueci-senha', element: <ForgotPasswordPage /> },
      { path: 'redefinir-senha', element: <ResetPasswordPage /> },
      {
        element: <RequireAuth />,
        children: [
          { path: 'dashboard', element: <DashboardPage /> },
          { path: 'cursos/novo', element: <CreateCoursePage /> },
        ],
      },
    ],
  },
])
