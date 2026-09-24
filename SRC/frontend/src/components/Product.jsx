import React, { useState } from 'react';
import axios from 'axios';
import { FiSearch, FiPackage, FiHash } from 'react-icons/fi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ;

function Product() {
  const [productName, setProductName] = useState('');
  const [products, setProducts] = useState([]);
  const [message, setMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSearch = async (e) => {
    e?.preventDefault();
    if (!productName) {
      setMessage('Please enter a product name to search.');
      return;
    }

    setIsLoading(true);
    setMessage('');
    try {
      const response = await axios.get(`${API_BASE_URL}/GetProduct/${productName}`);
      setProducts(response.data);
      setMessage(response.data.length ? '' : 'No products found matching your search.');
    } catch (error) {
      setProducts([]);
      setMessage('Error searching for products. Please try again.');
      console.error('Error searching for products:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div>
      {/* Page header */}
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <div style={{
            width: '38px', height: '38px',
            background: 'rgba(99,102,241,0.12)',
            border: '1px solid rgba(99,102,241,0.2)',
            borderRadius: '10px',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: '#818cf8',
          }}>
            <FiPackage size={18} />
          </div>
          <div>
            <h1 style={{ margin: 0, fontSize: '1.5rem', fontWeight: 800, color: '#f1f5f9', letterSpacing: '-0.02em' }}>
              Product Search
            </h1>
            <p style={{ margin: 0, fontSize: '0.85rem', color: '#64748b' }}>
              Find products by name or code
            </p>
          </div>
        </div>
      </div>

      {/* Search card */}
      <div className="glass-card" style={{ padding: '1.75rem', marginBottom: '1.25rem' }}>
        <form onSubmit={handleSearch}>
          <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 700, color: '#64748b', marginBottom: '0.6rem', textTransform: 'uppercase', letterSpacing: '0.07em' }}>
            Search Products
          </label>
          <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
            <div style={{ position: 'relative', flex: 1, minWidth: '200px' }}>
              <FiSearch size={15} style={{ position: 'absolute', left: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: '#475569', pointerEvents: 'none' }} />
              <input
                type="text"
                value={productName}
                onChange={(e) => setProductName(e.target.value)}
                placeholder="Enter product name or code..."
                className="input-field"
                disabled={isLoading}
                style={{ paddingLeft: '2.375rem' }}
              />
            </div>
            <button
              type="submit"
              disabled={isLoading}
              className="btn-primary"
              style={{ whiteSpace: 'nowrap', minWidth: '110px' }}
            >
              {isLoading ? (
                <>
                  <div className="spinner" />
                  Searching...
                </>
              ) : (
                <>
                  <FiSearch size={15} />
                  Search
                </>
              )}
            </button>
          </div>
        </form>
      </div>

      {/* Message */}
      {message && (
        <div className={products.length ? 'alert-success' : 'alert-error'} style={{ marginBottom: '1.25rem' }}>
          <span>{message}</span>
        </div>
      )}

      {/* Results */}
      {products.length > 0 && (
        <div className="glass-card" style={{ overflow: 'hidden' }}>
          <div style={{
            padding: '1rem 1.5rem',
            borderBottom: '1px solid #1e2d45',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.625rem' }}>
              <FiPackage size={16} style={{ color: '#818cf8' }} />
              <span style={{ fontWeight: 700, fontSize: '0.9rem', color: '#f1f5f9' }}>Search Results</span>
            </div>
            <span className="badge-success">
              {products.length} {products.length === 1 ? 'item' : 'items'} found
            </span>
          </div>

          <div style={{ overflowX: 'auto' }}>
            <table className="table-dark">
              <thead>
                <tr>
                  <th style={{ width: '50px', textAlign: 'center' }}>#</th>
                  <th>Product Name</th>
                  <th>Product Code</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product, idx) => (
                  <tr key={product.code}>
                    <td style={{ textAlign: 'center', color: '#475569', fontSize: '0.8rem', fontWeight: 600 }}>
                      {idx + 1}
                    </td>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        <div style={{ width: 7, height: 7, borderRadius: '50%', background: '#6366f1', flexShrink: 0 }} />
                        <span style={{ color: '#f1f5f9', fontWeight: 500 }}>{product.productName}</span>
                      </div>
                    </td>
                    <td>
                      <div style={{ display: 'inline-flex', alignItems: 'center', gap: '0.35rem', padding: '0.25rem 0.6rem', background: 'rgba(99,102,241,0.1)', border: '1px solid rgba(99,102,241,0.15)', borderRadius: '6px' }}>
                        <FiHash size={11} style={{ color: '#6366f1' }} />
                        <span style={{ fontSize: '0.8rem', color: '#818cf8', fontFamily: 'monospace', fontWeight: 600 }}>{product.code}</span>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

export default Product;
