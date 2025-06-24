/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GOOGLE_CLIENT_ID: string
  readonly VITE_API_BASE_URL: string
  readonly VITE_DEV_MODE: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

// Google OAuth types
declare global {
  interface Window {
    google: {
      accounts: {
        id: {
          initialize: (config: any) => void
          renderButton: (element: HTMLElement, options: any) => void
        }
      }
    }
  }
  
  const google: {
    accounts: {
      id: {
        initialize: (config: any) => void
        renderButton: (element: HTMLElement, options: any) => void
      }
    }
  }
} 