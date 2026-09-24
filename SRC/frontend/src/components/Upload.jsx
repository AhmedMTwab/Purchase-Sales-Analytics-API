import React, { useState, useRef } from 'react';
import axios from 'axios';
import { FiUpload, FiShoppingCart, FiDollarSign, FiCheckCircle, FiAlertCircle, FiUploadCloud, FiX, FiFile } from 'react-icons/fi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

function Upload() {
  const [purchaseFile, setPurchaseFile] = useState(null);
  const [saleFile, setSaleFile] = useState(null);
  const [purchaseMessage, setPurchaseMessage] = useState({ text: '', isError: false });
  const [saleMessage, setSaleMessage] = useState({ text: '', isError: false });
  const [isUploadingPurchase, setIsUploadingPurchase] = useState(false);
  const [isUploadingSale, setIsUploadingSale] = useState(false);
  const [purchaseDragOver, setPurchaseDragOver] = useState(false);
  const [saleDragOver, setSaleDragOver] = useState(false);

  const purchaseInputRef = useRef(null);
  const saleInputRef = useRef(null);

  const handleFileChange = (setter, msgSetter) => (e) => {
    const file = e.target.files[0];
    if (file) {
      setter(file);
      msgSetter({ text: '', isError: false });
    }
  };

  const handleDrop = (setter, msgSetter, setDrag) => (e) => {
    e.preventDefault();
    setDrag(false);
    const file = e.dataTransfer.files[0];
    if (file) {
      setter(file);
      msgSetter({ text: '', isError: false });
    }
  };

  const handlePurchaseUpload = async () => {
    if (!purchaseFile) {
      setPurchaseMessage({ text: 'Please select a file to upload.', isError: true });
      return;
    }
    const formData = new FormData();
    formData.append('purchaseFile', purchaseFile);
    setIsUploadingPurchase(true);
    try {
      const response = await axios.post(`${API_BASE_URL}/api/upload/Purchase`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setPurchaseMessage({ text: response.data || 'Purchase data uploaded successfully!', isError: false });
      setPurchaseFile(null);
      if (purchaseInputRef.current) purchaseInputRef.current.value = '';
    } catch (error) {
      console.error('Error uploading purchase file:', error);
      setPurchaseMessage({ text: error.response?.data?.message || 'Error uploading purchase file.', isError: true });
    } finally {
      setIsUploadingPurchase(false);
    }
  };

  const handleSaleUpload = async () => {
    if (!saleFile) {
      setSaleMessage({ text: 'Please select a file to upload.', isError: true });
      return;
    }
    const formData = new FormData();
    formData.append('salesFile', saleFile);
    setIsUploadingSale(true);
    try {
      const response = await axios.post(`${API_BASE_URL}/api/upload/Sale`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setSaleMessage({ text: response.data || 'Sales data uploaded successfully!', isError: false });
      setSaleFile(null);
      if (saleInputRef.current) saleInputRef.current.value = '';
    } catch (error) {
      console.error('Error uploading sale file:', error);
      setSaleMessage({ text: error.response?.data?.message || 'Error uploading sales file.', isError: true });
    } finally {
      setIsUploadingSale(false);
    }
  };

  const FileUploadCard = ({
    title, subtitle, icon, accentColor, file, message,
    onFileChange, onUpload, isUploading, inputRef, inputId,
    dragOver, setDragOver, onDrop
  }) => (
    <div className="glass-card" style={{ padding: '1.75rem' }}>
      {/* Card header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.875rem', marginBottom: '1.5rem' }}>
        <div style={{
          width: '46px', height: '46px',
          background: `${accentColor}18`,
          border: `1px solid ${accentColor}30`,
          borderRadius: '12px',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: accentColor,
        }}>
          {icon}
        </div>
        <div>
          <h3 style={{ margin: 0, fontWeight: 700, fontSize: '1rem', color: '#f1f5f9' }}>{title}</h3>
          <p style={{ margin: 0, fontSize: '0.8rem', color: '#64748b' }}>{subtitle}</p>
        </div>
      </div>

      {/* Drop zone */}
      <label
        htmlFor={inputId}
        className={`drop-zone ${dragOver ? 'active' : ''}`}
        onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
        onDragLeave={() => setDragOver(false)}
        onDrop={onDrop}
        style={{ cursor: 'pointer' }}
      >
        <div style={{
          width: '52px', height: '52px',
          background: `${accentColor}12`,
          border: `1px solid ${accentColor}25`,
          borderRadius: '14px',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          marginBottom: '0.875rem',
          color: accentColor,
          transition: 'all 0.25s ease',
        }}>
          <FiUploadCloud size={22} />
        </div>
        <p style={{ margin: '0 0 0.3rem 0', fontSize: '0.875rem', fontWeight: 600, color: '#cbd5e1' }}>
          <span style={{ color: accentColor }}>Click to browse</span> or drag & drop
        </p>
        <p style={{ margin: 0, fontSize: '0.75rem', color: '#475569' }}>CSV, XLSX or XLS · max 10 MB</p>
        <input
          ref={inputRef}
          id={inputId}
          type="file"
          onChange={onFileChange}
          style={{ display: 'none' }}
          accept=".csv,.xlsx,.xls"
        />
      </label>

      {/* File selected */}
      {file && (
        <div style={{
          marginTop: '0.875rem',
          padding: '0.75rem 1rem',
          background: 'rgba(16,185,129,0.07)',
          border: '1px solid rgba(16,185,129,0.2)',
          borderRadius: '10px',
          display: 'flex',
          alignItems: 'center',
          gap: '0.75rem',
        }}>
          <FiFile size={16} style={{ color: '#34d399', flexShrink: 0 }} />
          <div style={{ flex: 1, minWidth: 0 }}>
            <p style={{ margin: 0, fontSize: '0.82rem', fontWeight: 600, color: '#6ee7b7', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {file.name}
            </p>
            <p style={{ margin: 0, fontSize: '0.72rem', color: '#475569', marginTop: '0.1rem' }}>
              {(file.size / 1024).toFixed(1)} KB
            </p>
          </div>
          <button
            onClick={() => {
              if (inputId === 'purchase-file') { setPurchaseFile(null); if (purchaseInputRef.current) purchaseInputRef.current.value = ''; }
              else { setSaleFile(null); if (saleInputRef.current) saleInputRef.current.value = ''; }
            }}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#475569', padding: '2px', display: 'flex', alignItems: 'center' }}
          >
            <FiX size={14} />
          </button>
        </div>
      )}

      {/* Upload button */}
      <button
        onClick={onUpload}
        disabled={!file || isUploading}
        className="btn-primary"
        style={{ width: '100%', marginTop: '1rem', padding: '0.75rem', fontSize: '0.9rem' }}
      >
        {isUploading ? (
          <>
            <div className="spinner" />
            Uploading...
          </>
        ) : (
          <>
            <FiUpload size={16} />
            Upload {title.replace('Upload ', '')}
          </>
        )}
      </button>

      {/* Status message */}
      {message.text && (
        <div className={message.isError ? 'alert-error' : 'alert-success'} style={{ marginTop: '0.875rem' }}>
          {message.isError
            ? <FiAlertCircle size={15} style={{ flexShrink: 0 }} />
            : <FiCheckCircle size={15} style={{ flexShrink: 0 }} />
          }
          <span>{message.text}</span>
        </div>
      )}
    </div>
  );

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
            <FiUploadCloud size={18} />
          </div>
          <div>
            <h1 style={{ margin: 0, fontSize: '1.5rem', fontWeight: 800, color: '#f1f5f9', letterSpacing: '-0.02em' }}>
              Data Upload
            </h1>
            <p style={{ margin: 0, fontSize: '0.85rem', color: '#64748b' }}>
              Upload purchase and sales records to update the system
            </p>
          </div>
        </div>
      </div>

      {/* Upload cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.25rem', marginBottom: '1.5rem' }}>
        <FileUploadCard
          title="Upload Purchases"
          subtitle="Import purchase order records"
          icon={<FiShoppingCart size={20} />}
          accentColor="#6366f1"
          file={purchaseFile}
          message={purchaseMessage}
          onFileChange={handleFileChange(setPurchaseFile, setPurchaseMessage)}
          onUpload={handlePurchaseUpload}
          isUploading={isUploadingPurchase}
          inputRef={purchaseInputRef}
          inputId="purchase-file"
          dragOver={purchaseDragOver}
          setDragOver={setPurchaseDragOver}
          onDrop={handleDrop(setPurchaseFile, setPurchaseMessage, setPurchaseDragOver)}
        />

        <FileUploadCard
          title="Upload Sales"
          subtitle="Import sales transaction records"
          icon={<FiDollarSign size={20} />}
          accentColor="#10b981"
          file={saleFile}
          message={saleMessage}
          onFileChange={handleFileChange(setSaleFile, setSaleMessage)}
          onUpload={handleSaleUpload}
          isUploading={isUploadingSale}
          inputRef={saleInputRef}
          inputId="sale-file"
          dragOver={saleDragOver}
          setDragOver={setSaleDragOver}
          onDrop={handleDrop(setSaleFile, setSaleMessage, setSaleDragOver)}
        />
      </div>

      {/* Requirements note */}
      <div style={{
        padding: '1rem 1.25rem',
        background: 'rgba(255,255,255,0.02)',
        border: '1px solid #1e2d45',
        borderRadius: '12px',
        display: 'flex',
        gap: '1rem',
        flexWrap: 'wrap',
      }}>
        {[
          { label: 'Format', value: 'CSV, XLSX, XLS' },
          { label: 'Max Size', value: '10 MB' },
          { label: 'Encoding', value: 'UTF-8' },
        ].map(({ label, value }) => (
          <div key={label} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span style={{ fontSize: '0.75rem', fontWeight: 600, color: '#475569', textTransform: 'uppercase', letterSpacing: '0.06em' }}>{label}:</span>
            <span style={{ fontSize: '0.8rem', fontWeight: 600, color: '#818cf8' }}>{value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Upload;
