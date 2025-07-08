import React, { useState } from 'react';
import InputForm from './components/InputForm';
import ResultsChart from './components/ResultsChart';
import ResultsTable from './components/ResultsTable';

function App() {
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  return (
    <div className="container" style={{ padding: '2rem' }}>
      <h1>NPV Calculator</h1>
      <InputForm
        onSubmitStart={() => {
          setLoading(true);
          setError('');
        }}
        onSubmitComplete={(data) => {
          setResults(data);
          setLoading(false);
        }}
        onSubmitError={(err) => {
          setError(err.message || 'Something went wrong');
          setLoading(false);
        }}
      />
      {loading && <p>Calculating...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {results.length > 0 && (
        <>
          <ResultsChart data={results} />
          <ResultsTable data={results} />
        </>
      )}
    </div>
  );
}

export default App;
