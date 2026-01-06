import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import CourseEditor from './pages/CourseEditor';
import CourseViewer from './pages/CourseViewer';
import 'bootstrap/dist/css/bootstrap.min.css';
import './premium.css';
import './fix-center.css';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/editor/:id" element={<CourseEditor />} />
        <Route path="/course/:id" element={<CourseViewer />} />
        <Route path="/" element={<Navigate to="/login" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
