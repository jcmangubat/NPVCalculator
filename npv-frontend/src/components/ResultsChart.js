import React from 'react';
import {
  LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip, ResponsiveContainer
} from 'recharts';

function ResultsChart({ data }) {
  return (
    <div style={{ marginBottom: '2rem' }}>
      <h2>NPV Chart</h2>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <Line type="monotone" dataKey="npv" stroke="#007bff" strokeWidth={2} />
          <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
          <XAxis dataKey="discountRate" label={{ value: 'Discuont Rate (%)', position: 'insideBottomRight', offset: -5 }} />
          <YAxis label={{ value: 'NPV', angle: -90, position: 'insideLeft' }} />
          <Tooltip formatter={(value) => value.toFixed(2)} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}

export default ResultsChart;
