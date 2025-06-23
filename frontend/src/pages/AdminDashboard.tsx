import { useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { 
  CogIcon,
  UserGroupIcon,
  BuildingOfficeIcon,
  ChartBarIcon,
  CpuChipIcon,
  KeyIcon,
  CheckCircleIcon,
  XCircleIcon,
  ExclamationTriangleIcon
} from '@heroicons/react/24/outline'

interface AIProvider {
  id: string
  name: string
  type: 'openai' | 'azure_openai' | 'claude' | 'gemini' | 'llama' | 'custom'
  enabled: boolean
  config: {
    apiKey?: string
    endpoint?: string
    model?: string
    temperature?: number
    maxTokens?: number
    region?: string
    version?: string
  }
  status: 'active' | 'inactive' | 'error'
  lastTested?: string
}

interface Tenant {
  id: string
  name: string
  domain: string
  users: number
  aiProvider: string
  plan: 'free' | 'pro' | 'enterprise'
  status: 'active' | 'suspended'
  createdAt: string
}

const AdminDashboard = () => {
  const { user, logout, isSystemAdmin } = useAuth()
  const [activeTab, setActiveTab] = useState<'overview' | 'ai-providers' | 'tenants' | 'settings'>('overview')
  const [showAddProvider, setShowAddProvider] = useState(false)
  const [selectedProvider, setSelectedProvider] = useState<AIProvider | null>(null)

  // Mock data for demo
  const [aiProviders, setAiProviders] = useState<AIProvider[]>([
    {
      id: 'openai-1',
      name: 'OpenAI GPT-4',
      type: 'openai',
      enabled: true,
      config: {
        apiKey: 'sk-proj-***************',
        model: 'gpt-4-turbo-preview',
        temperature: 0.7,
        maxTokens: 4000
      },
      status: 'active',
      lastTested: '2024-01-13T10:30:00Z'
    },
    {
      id: 'azure-1',
      name: 'Azure OpenAI',
      type: 'azure_openai',
      enabled: false,
      config: {
        apiKey: '***************',
        endpoint: 'https://your-resource.openai.azure.com/',
        model: 'gpt-4',
        version: '2023-12-01-preview'
      },
      status: 'inactive'
    },
    {
      id: 'claude-1',
      name: 'Anthropic Claude',
      type: 'claude',
      enabled: true,
      config: {
        apiKey: 'sk-ant-***************',
        model: 'claude-3-opus-20240229',
        temperature: 0.7,
        maxTokens: 2000
      },
      status: 'error',
      lastTested: '2024-01-13T09:15:00Z'
    }
  ])

  const [tenants] = useState<Tenant[]>([
    {
      id: 'tenant_1',
      name: 'Acme Corp',
      domain: 'acme.com',
      users: 45,
      aiProvider: 'openai-1',
      plan: 'enterprise',
      status: 'active',
      createdAt: '2024-01-01'
    },
    {
      id: 'tenant_2',
      name: 'StartupXYZ',
      domain: 'startupxyz.com',
      users: 12,
      aiProvider: 'claude-1',
      plan: 'pro',
      status: 'active',
      createdAt: '2024-01-10'
    },
    {
      id: 'tenant_3',
      name: 'TechCo',
      domain: 'techco.io',
      users: 8,
      aiProvider: 'openai-1',
      plan: 'free',
      status: 'suspended',
      createdAt: '2024-01-12'
    }
  ])

  if (!isSystemAdmin()) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <ExclamationTriangleIcon className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Access Denied</h1>
          <p className="text-gray-600">You need system administrator privileges to access this page.</p>
        </div>
      </div>
    )
  }

  const getProviderIcon = (type: AIProvider['type']) => {
    return <CpuChipIcon className="w-6 h-6" />
  }

  const getStatusIcon = (status: AIProvider['status']) => {
    switch (status) {
      case 'active':
        return <CheckCircleIcon className="w-5 h-5 text-green-500" />
      case 'error':
        return <XCircleIcon className="w-5 h-5 text-red-500" />
      default:
        return <XCircleIcon className="w-5 h-5 text-gray-400" />
    }
  }

  const testProvider = async (provider: AIProvider) => {
    // Mock testing
    const updatedProviders = aiProviders.map(p => 
      p.id === provider.id 
        ? { ...p, status: 'active' as const, lastTested: new Date().toISOString() }
        : p
    )
    setAiProviders(updatedProviders)
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <CogIcon className="w-8 h-8 text-primary-600 mr-2" />
              <h1 className="text-2xl font-bold text-primary-600">System Administration</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-gray-700">System Admin: {user?.name}</span>
              <button
                onClick={logout}
                className="text-gray-500 hover:text-gray-700"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Tab Navigation */}
        <div className="border-b border-gray-200 mb-8">
          <nav className="-mb-px flex space-x-8">
            {[
              { id: 'overview', name: 'Overview', icon: ChartBarIcon },
              { id: 'ai-providers', name: 'AI Providers', icon: CpuChipIcon },
              { id: 'tenants', name: 'Tenants', icon: BuildingOfficeIcon },
              { id: 'settings', name: 'Settings', icon: CogIcon }
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as any)}
                className={`${
                  activeTab === tab.id
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm flex items-center`}
              >
                <tab.icon className="w-5 h-5 mr-2" />
                {tab.name}
              </button>
            ))}
          </nav>
        </div>

        {/* Overview Tab */}
        {activeTab === 'overview' && (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div className="bg-white p-6 rounded-lg shadow-sm">
                <div className="flex items-center">
                  <BuildingOfficeIcon className="h-8 w-8 text-blue-600" />
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-500">Total Tenants</p>
                    <p className="text-2xl font-semibold text-gray-900">{tenants.length}</p>
                  </div>
                </div>
              </div>
              <div className="bg-white p-6 rounded-lg shadow-sm">
                <div className="flex items-center">
                  <UserGroupIcon className="h-8 w-8 text-green-600" />
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-500">Total Users</p>
                    <p className="text-2xl font-semibold text-gray-900">
                      {tenants.reduce((sum, t) => sum + t.users, 0)}
                    </p>
                  </div>
                </div>
              </div>
              <div className="bg-white p-6 rounded-lg shadow-sm">
                <div className="flex items-center">
                  <CpuChipIcon className="h-8 w-8 text-purple-600" />
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-500">AI Providers</p>
                    <p className="text-2xl font-semibold text-gray-900">{aiProviders.length}</p>
                  </div>
                </div>
              </div>
              <div className="bg-white p-6 rounded-lg shadow-sm">
                <div className="flex items-center">
                  <CheckCircleIcon className="h-8 w-8 text-emerald-600" />
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-500">Active Providers</p>
                    <p className="text-2xl font-semibold text-gray-900">
                      {aiProviders.filter(p => p.status === 'active').length}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Recent Activity */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h3>
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">OpenAI GPT-4 provider tested successfully</span>
                  <span className="text-xs text-gray-400">2 hours ago</span>
                </div>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">New tenant "TechCo" added</span>
                  <span className="text-xs text-gray-400">1 day ago</span>
                </div>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">Claude provider configuration updated</span>
                  <span className="text-xs text-gray-400">2 days ago</span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* AI Providers Tab */}
        {activeTab === 'ai-providers' && (
          <div className="space-y-6">
            <div className="flex justify-between items-center">
              <h2 className="text-2xl font-bold text-gray-900">AI Provider Management</h2>
              <button
                onClick={() => setShowAddProvider(true)}
                className="btn-primary"
              >
                Add Provider
              </button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {aiProviders.map((provider) => (
                <div key={provider.id} className="bg-white rounded-lg shadow-sm p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center">
                      {getProviderIcon(provider.type)}
                      <div className="ml-3">
                        <h3 className="text-lg font-semibold text-gray-900">{provider.name}</h3>
                        <p className="text-sm text-gray-500 capitalize">{provider.type.replace('_', ' ')}</p>
                      </div>
                    </div>
                    <div className="flex items-center space-x-2">
                      {getStatusIcon(provider.status)}
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                        provider.enabled 
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-gray-100 text-gray-800'
                      }`}>
                        {provider.enabled ? 'Enabled' : 'Disabled'}
                      </span>
                    </div>
                  </div>

                  <div className="space-y-2 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Model:</span>
                      <span className="text-gray-900">{provider.config.model || 'Not set'}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Temperature:</span>
                      <span className="text-gray-900">{provider.config.temperature || 'Default'}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Max Tokens:</span>
                      <span className="text-gray-900">{provider.config.maxTokens || 'Default'}</span>
                    </div>
                    {provider.lastTested && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-500">Last Tested:</span>
                        <span className="text-gray-900">
                          {new Date(provider.lastTested).toLocaleDateString()}
                        </span>
                      </div>
                    )}
                  </div>

                  <div className="flex space-x-2">
                    <button
                      onClick={() => testProvider(provider)}
                      className="btn-secondary text-sm"
                    >
                      Test Connection
                    </button>
                    <button
                      onClick={() => setSelectedProvider(provider)}
                      className="btn-secondary text-sm"
                    >
                      Configure
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Tenants Tab */}
        {activeTab === 'tenants' && (
          <div className="space-y-6">
            <div className="flex justify-between items-center">
              <h2 className="text-2xl font-bold text-gray-900">Tenant Management</h2>
            </div>

            <div className="bg-white rounded-lg shadow-sm overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tenant
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Users
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      AI Provider
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Plan
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {tenants.map((tenant) => (
                    <tr key={tenant.id}>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div className="text-sm font-medium text-gray-900">{tenant.name}</div>
                          <div className="text-sm text-gray-500">{tenant.domain}</div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {tenant.users}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {aiProviders.find(p => p.id === tenant.aiProvider)?.name || 'Not set'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                          tenant.plan === 'enterprise' ? 'bg-purple-100 text-purple-800' :
                          tenant.plan === 'pro' ? 'bg-blue-100 text-blue-800' :
                          'bg-gray-100 text-gray-800'
                        }`}>
                          {tenant.plan}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                          tenant.status === 'active' 
                            ? 'bg-green-100 text-green-800' 
                            : 'bg-red-100 text-red-800'
                        }`}>
                          {tenant.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        <button className="text-primary-600 hover:text-primary-900">
                          Manage
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Configure Provider Modal */}
        {selectedProvider && (
          <>
            <div className="fixed inset-0 bg-black bg-opacity-50 z-40" onClick={() => setSelectedProvider(null)} />
            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6">
                <h3 className="text-2xl font-bold text-gray-900 mb-6">Configure {selectedProvider.name}</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Display Name
                    </label>
                    <input
                      type="text"
                      className="input-field"
                      defaultValue={selectedProvider.name}
                      placeholder="e.g., Google Gemini Pro, OpenAI GPT-4"
                    />
                    <p className="mt-1 text-xs text-gray-500">
                      A friendly name to identify this configuration in your system
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      API Key
                    </label>
                    <input
                      type="password"
                      className="input-field"
                      defaultValue={selectedProvider.config.apiKey}
                      placeholder="Enter your API key"
                    />
                    <p className="mt-1 text-xs text-gray-500">
                      Your secret API key from the provider's console
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Model Selection
                    </label>
                    <select className="input-field" defaultValue={selectedProvider.config.model}>
                      <option value="">Select a model</option>
                      {selectedProvider.type === 'openai' && (
                        <>
                          <option value="gpt-4-turbo-preview">GPT-4 Turbo</option>
                          <option value="gpt-4">GPT-4</option>
                          <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
                        </>
                      )}
                      {selectedProvider.type === 'gemini' && (
                        <>
                          <option value="gemini-pro">Gemini Pro</option>
                          <option value="gemini-pro-vision">Gemini Pro Vision</option>
                        </>
                      )}
                      {selectedProvider.type === 'claude' && (
                        <>
                          <option value="claude-3-opus-20240229">Claude 3 Opus</option>
                          <option value="claude-3-sonnet-20240229">Claude 3 Sonnet</option>
                          <option value="claude-3-haiku-20240307">Claude 3 Haiku</option>
                        </>
                      )}
                    </select>
                    <p className="mt-1 text-xs text-gray-500">
                      Choose the specific model version to use
                    </p>
                  </div>
                  {selectedProvider.type === 'azure_openai' && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Azure Endpoint
                      </label>
                      <input
                        type="text"
                        className="input-field"
                        defaultValue={selectedProvider.config.endpoint}
                        placeholder="https://your-resource.openai.azure.com/"
                      />
                      <p className="mt-1 text-xs text-gray-500">
                        Your Azure OpenAI resource endpoint
                      </p>
                    </div>
                  )}
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Temperature
                      </label>
                      <input
                        type="number"
                        step="0.1"
                        min="0"
                        max="2"
                        className="input-field"
                        defaultValue={selectedProvider.config.temperature || 0.7}
                      />
                      <p className="mt-1 text-xs text-gray-500">
                        Creativity (0-2)
                      </p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Max Tokens
                      </label>
                      <input
                        type="number"
                        className="input-field"
                        defaultValue={selectedProvider.config.maxTokens || 2000}
                      />
                      <p className="mt-1 text-xs text-gray-500">
                        Response length limit
                      </p>
                    </div>
                  </div>
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                        defaultChecked={selectedProvider.enabled}
                      />
                      <span className="ml-2 text-sm text-gray-700">Enable this provider</span>
                    </label>
                  </div>
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      onClick={() => setSelectedProvider(null)}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button 
                      onClick={() => {
                        testProvider(selectedProvider)
                        setSelectedProvider(null)
                      }}
                      className="btn-primary"
                    >
                      Save Configuration
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}

        {/* Add Provider Modal */}
        {showAddProvider && (
          <>
            <div className="fixed inset-0 bg-black bg-opacity-50 z-40" onClick={() => setShowAddProvider(false)} />
            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6">
                <h3 className="text-2xl font-bold text-gray-900 mb-6">Add AI Provider</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Provider Type
                    </label>
                    <select className="input-field">
                      <option value="openai">OpenAI</option>
                      <option value="azure_openai">Azure OpenAI</option>
                      <option value="claude">Anthropic Claude</option>
                      <option value="gemini">Google Gemini</option>
                      <option value="llama">Meta Llama</option>
                      <option value="custom">Custom Provider</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Display Name
                    </label>
                    <input
                      type="text"
                      className="input-field"
                      placeholder="e.g., Google Gemini Pro, OpenAI GPT-4"
                    />
                    <p className="mt-1 text-xs text-gray-500">
                      A friendly name to identify this configuration in your system
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      API Key
                    </label>
                    <input
                      type="password"
                      className="input-field"
                      placeholder="Enter your API key"
                    />
                    <p className="mt-1 text-xs text-gray-500">
                      Your secret API key from the provider's console
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Model Selection
                    </label>
                    <select className="input-field">
                      <option value="">Select a model</option>
                      <optgroup label="OpenAI">
                        <option value="gpt-4-turbo-preview">GPT-4 Turbo</option>
                        <option value="gpt-4">GPT-4</option>
                        <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
                      </optgroup>
                      <optgroup label="Google Gemini">
                        <option value="gemini-pro">Gemini Pro</option>
                        <option value="gemini-pro-vision">Gemini Pro Vision</option>
                      </optgroup>
                      <optgroup label="Claude">
                        <option value="claude-3-opus-20240229">Claude 3 Opus</option>
                        <option value="claude-3-sonnet-20240229">Claude 3 Sonnet</option>
                        <option value="claude-3-haiku-20240307">Claude 3 Haiku</option>
                      </optgroup>
                    </select>
                    <p className="mt-1 text-xs text-gray-500">
                      Choose the specific model version to use
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      API Endpoint (Optional)
                    </label>
                    <input
                      type="text"
                      className="input-field"
                      placeholder="e.g., https://api.openai.com/v1 (leave blank for default)"
                    />
                    <p className="mt-1 text-xs text-gray-500">
                      Only needed for Azure OpenAI or custom endpoints
                    </p>
                  </div>
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      onClick={() => setShowAddProvider(false)}
                      className="btn-secondary"
                    >
                      Cancel
                    </button>
                    <button className="btn-primary">
                      Add Provider
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default AdminDashboard