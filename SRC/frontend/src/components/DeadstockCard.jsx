import React, { useState } from 'react';
import axios from 'axios';
import { FiAlertTriangle } from 'react-icons/fi';
import StatCard from './StatCard';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5198';

const DeadstockCard = () => {
  const [deadstock, setDeadstock] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleGetDeadstock = async () => {
    setIsLoading(true);
    try {
      const response = await axios.get(`${API_BASE_URL}/GetDeadstock`);
      setDeadstock(response.data);
      setMessage(response.data.length ? '' : 'No deadstock items found. Inventory looks healthy!');
    } catch (error) {
      setDeadstock([]);
      setMessage(error.response?.data?.message || 'Error fetching deadstock. Please try again.');
      console.error('Error fetching deadstock:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <StatCard
      title="Deadstock Analysis"
      icon={<FiAlertTriangle size={18} />}
      message={message}
      actionLabel="Scan Inventory"
      isLoading={isLoading}
      onAction={handleGetDeadstock}
    >
      {deadstock.length > 0 && (
        <div style={{ marginTop: '1rem' }}>
          <div className="divider" style={{ margin: '0 0 1rem 0' }} />

          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '0.75rem' }}>
            <p className="section-label" style={{ margin: 0 }}>
              {deadstock.length} {deadstock.length === 1 ? 'item' : 'items'} found
            </p>
            <span className="badge-danger">
              <FiAlertTriangle size={10} />
              Attention
            </span>
          </div>

          <div style={{
            border: '1px solid #1e2d45',
            borderRadius: '10px',
            overflow: 'hidden',
            maxHeight: '220px',
          }}>
            {deadstock.map((product, index) => (
              <div key={index} style={{
                padding: '0.65rem 0.875rem',
                fontSize: '0.85rem',
                color: '#94a3b8',
                borderBottom: index < deadstock.length - 1 ? '1px solid rgba(30,45,69,0.5)' : 'none',
                background: 'rgba(239,68,68,0.02)',
                display: 'flex',
                alignItems: 'center',
                gap: '0.625rem',
              }}>
                <div style={{ width: 6, height: 6, borderRadius: '50%', background: '#ef4444', flexShrink: 0, opacity: 0.7 }} />
                <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{product}</span>
              </div>
            ))}
          </div>

          <p style={{ marginTop: '0.75rem', fontSize: '0.75rem', color: '#475569', lineHeight: 1.5 }}>
            💡 Consider discounting or bundling these items to clear inventory.
          </p>
        </div>
      )}
    </StatCard>
  );
};

export default DeadstockCard;
