const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7030/api/npv';

// Endpoint to calcilate for NPVs
export const calculateNpv = async (payload) => {
  const response = await fetch(`${API_BASE_URL}/calculate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || 'Failed to calculate NPV');
  }

  return response.json();
};

// Endpoint to download csv
export const downloadCsv = async (payload) => {
  const response = await fetch(`${API_BASE_URL}/calculate/csv`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (!response.ok) throw new Error('Failed to download CSV');

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;

  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  link.download = `npv-results-${timestamp}.csv`;
  link.click();
};