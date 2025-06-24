import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';

const GoogleLoginBypass: React.FC = () => {
  const { bypassLogin } = useAuth();

  const handleBypassLogin = async (role: 'user' | 'admin') => {
    try {
      await bypassLogin(role);
      toast.success(`Logged in as ${role} (bypass mode)`);
    } catch (error) {
      console.error('Bypass login error:', error);
      toast.error('Failed to bypass login');
    }
  };

  return (
    <div className="mt-4 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
      <p className="text-sm text-yellow-800 mb-3">
        <strong>Development Mode:</strong> Google OAuth configuration issue detected.
        Use bypass login for testing.
      </p>
      <div className="flex space-x-2">
        <button
          onClick={() => handleBypassLogin('user')}
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md text-sm"
        >
          Login as User
        </button>
        <button
          onClick={() => handleBypassLogin('admin')}
          className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-md text-sm"
        >
          Login as Admin
        </button>
      </div>
    </div>
  );
};

export default GoogleLoginBypass;