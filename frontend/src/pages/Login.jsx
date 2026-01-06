import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { FaEnvelope, FaLock, FaSignInAlt, FaUserPlus } from 'react-icons/fa';

export default function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        try {
            const res = await api.post('/auth/login', { email, password });
            localStorage.setItem('token', res.data.token);
            navigate('/dashboard');
        } catch (err) {
            alert('Error al iniciar sesión. Verifica tus credenciales.');
        }
    };

    return (
        <div className="min-vh-100 d-flex align-items-center justify-content-center">
            <div className="premium-card p-5" style={{ width: '450px', margin: '0 auto' }}>
                <div className="text-center mb-4">
                    <h2 className="mb-2">Iniciar Sesión</h2>
                    <p className="text-gold">Plataforma de Gestión de Cursos</p>
                </div>
                <form onSubmit={handleLogin}>
                    <div className="mb-3">
                        <label className="form-label text-gold">Correo Electrónico</label>
                        <div className="input-group">
                            <span className="input-group-text bg-dark border-secondary text-gold">
                                <FaEnvelope />
                            </span>
                            <input
                                type="email"
                                className="form-control"
                                placeholder="tu@email.com"
                                value={email}
                                onChange={e => setEmail(e.target.value)}
                                required
                            />
                        </div>
                    </div>
                    <div className="mb-4">
                        <label className="form-label text-gold">Contraseña</label>
                        <div className="input-group">
                            <span className="input-group-text bg-dark border-secondary text-gold">
                                <FaLock />
                            </span>
                            <input
                                type="password"
                                className="form-control"
                                placeholder="********"
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                required
                            />
                        </div>
                    </div>
                    <button className="btn btn-gold w-100 mb-3 py-2 d-flex align-items-center justify-content-center gap-2">
                        <FaSignInAlt /> Ingresar
                    </button>
                    <div className="text-center">
                        <button
                            type="button"
                            className="btn btn-link text-gold d-flex align-items-center justify-content-center gap-2 mx-auto"
                            onClick={() => navigate('/register')}
                        >
                            <FaUserPlus /> ¿No tienes cuenta? Regístrate aquí
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
