import React, { useState } from 'react';
import { jobsApi } from '../services/api';

const ChannelCard = ({ channel, onJobTriggered }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleTrigger = async () => {
    setIsLoading(true);
    setError(null);
    try {
      await jobsApi.trigger(channel.id);
      if (onJobTriggered) {
        onJobTriggered();
      }
    } catch (err) {
      setError(err.response?.data || 'Failed to trigger job');
      console.error('Error triggering job:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-xl font-semibold text-gray-800">{channel.name}</h3>
          <p className="text-sm text-gray-500 mt-1">
            {channel.niche?.name || 'No Niche'}
          </p>
        </div>
        <span
          className={`px-3 py-1 rounded-full text-xs font-medium ${
            channel.isActive
              ? 'bg-green-100 text-green-800'
              : 'bg-gray-100 text-gray-800'
          }`}
        >
          {channel.isActive ? 'Active' : 'Paused'}
        </span>
      </div>

      <div className="space-y-2 mb-4">
        <div className="flex items-center text-sm text-gray-600">
          <span className="font-medium mr-2">Platform:</span>
          <span>{channel.platform}</span>
        </div>
        {channel.scheduleCron && (
          <div className="flex items-center text-sm text-gray-600">
            <span className="font-medium mr-2">Schedule:</span>
            <span className="font-mono text-xs">{channel.scheduleCron}</span>
          </div>
        )}
      </div>

      {error && (
        <div className="mb-3 p-2 bg-red-50 text-red-700 text-sm rounded">
          {error}
        </div>
      )}

      <button
        onClick={handleTrigger}
        disabled={!channel.isActive || isLoading}
        className={`w-full py-2 px-4 rounded-md font-medium transition-colors ${
          channel.isActive && !isLoading
            ? 'bg-blue-600 text-white hover:bg-blue-700'
            : 'bg-gray-300 text-gray-500 cursor-not-allowed'
        }`}
      >
        {isLoading ? 'Triggering...' : 'Trigger Now'}
      </button>
    </div>
  );
};

export default ChannelCard;
