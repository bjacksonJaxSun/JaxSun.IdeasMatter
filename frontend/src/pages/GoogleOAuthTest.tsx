import React, { useEffect, useState } from 'react';
import { GoogleLogin } from '@react-oauth/google';

const GoogleOAuthTest: React.FC = () => {
  const [status, setStatus] = useState<string>('Loading...');
  const [token, setToken] = useState<string>('');
  const [error, setError] = useState<string>('');

  useEffect(() => {
    // Check if Google OAuth library is loaded
    const checkGoogleOAuth = () => {
      if (typeof google !== 'undefined' && google.accounts) {
        setStatus('Google OAuth library loaded successfully');
        console.log('✅ Google OAuth library loaded successfully');
      } else {
        setStatus('Google OAuth library not loaded');
        setError('Check your internet connection and try refreshing the page.');
        console.error('❌ Google OAuth library not loaded');
      }
    };

    // Wait a bit for the library to load
    setTimeout(checkGoogleOAuth, 2000);
  }, []);

  const handleSuccess = (credentialResponse: any) => {
    console.log("✅ Google OAuth Success!");
    console.log("Token:", credentialResponse.credential);
    setToken(credentialResponse.credential);
    setStatus('Google OAuth Working!');
    setError('');
  };

  const handleError = () => {
    console.error("❌ Google OAuth Error");
    setError('Google OAuth failed. Check the console for details.');
    setStatus('Google OAuth Failed');
  };

  const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || '';

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-6">
            Google OAuth Configuration Test
          </h1>
          
          {/* Configuration Info */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <h2 className="text-lg font-semibold text-blue-900 mb-2">Configuration</h2>
            <div className="space-y-2 text-sm">
              <div><strong>Client ID:</strong> {googleClientId || 'Not set'}</div>
              <div><strong>Current URL:</strong> {window.location.href}</div>
              <div><strong>Environment:</strong> {import.meta.env.MODE}</div>
            </div>
          </div>

          {/* Status */}
          <div className={`p-4 rounded-lg mb-6 ${
            error ? 'bg-red-50 border border-red-200' : 
            status.includes('Working') ? 'bg-green-50 border border-green-200' :
            'bg-yellow-50 border border-yellow-200'
          }`}>
            <h2 className="text-lg font-semibold mb-2">Status</h2>
            <p className={error ? 'text-red-700' : status.includes('Working') ? 'text-green-700' : 'text-yellow-700'}>
              {status}
            </p>
            {error && <p className="text-red-600 mt-2">{error}</p>}
          </div>

          {/* Google Login Button */}
          <div className="flex justify-center mb-6">
            <GoogleLogin
              onSuccess={handleSuccess}
              onError={handleError}
              useOneTap={false}
              theme="outline"
              size="large"
              type="standard"
              locale="en"
            />
          </div>

          {/* Token Display */}
          {token && (
            <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-2">Token Received</h2>
              <p className="text-sm text-gray-600 mb-2">
                <strong>Token length:</strong> {token.length} characters
              </p>
              <details className="text-sm">
                <summary className="cursor-pointer text-blue-600 hover:text-blue-800">
                  View token (first 50 characters)
                </summary>
                <pre className="mt-2 p-2 bg-gray-100 rounded text-xs overflow-x-auto">
                  {token.substring(0, 50)}...
                </pre>
              </details>
            </div>
          )}

          {/* Troubleshooting */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <h2 className="text-lg font-semibold text-blue-900 mb-2">Troubleshooting Steps</h2>
            <ol className="list-decimal list-inside space-y-1 text-sm text-blue-800">
              <li>If you see "Access Blocked" error, go to Google Cloud Console</li>
              <li>Add <code className="bg-blue-100 px-1 rounded">http://localhost:4000</code> to Authorized JavaScript origins</li>
              <li>Add <code className="bg-blue-100 px-1 rounded">http://localhost:4000</code> to Authorized redirect URIs</li>
              <li>Clear browser cache and try again</li>
              <li>Check browser console for detailed error messages</li>
            </ol>
          </div>

          {/* Back to App */}
          <div className="mt-6 text-center">
            <a 
              href="/" 
              className="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors"
            >
              ← Back to Ideas Matter
            </a>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoogleOAuthTest; 