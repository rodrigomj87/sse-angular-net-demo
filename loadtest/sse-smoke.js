import http from 'k6/http';
import { sleep } from 'k6';

export const options = { vus: 5, duration: '10s' };

export default function () {
  const res = http.get('http://localhost:5000/sse/stream');
  // connection should stay open; we just sleep then end iteration
  sleep(1);
}
