import { useEffect, useState } from 'react';
import api from '../api/axios';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { FaPlus, FaSignOutAlt, FaEdit, FaTrash, FaEye, FaChevronLeft, FaChevronRight } from 'react-icons/fa';

export default function Dashboard() {
    const [courses, setCourses] = useState([]);
    const [newTitle, setNewTitle] = useState('');
    const [page, setPage] = useState(1);
    const [statusFilter, setStatusFilter] = useState('');
    const [isAdmin, setIsAdmin] = useState(false);
    const [canCreate, setCanCreate] = useState(false);
    const [currentUserId, setCurrentUserId] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decoded = jwtDecode(token);
                // Check for standard role claim or short role claim
                console.log('Decoded Token:', decoded);
                let roles = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                    decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] ||
                    decoded['role'];
                if (typeof roles === 'string') roles = [roles];
                if (!roles) roles = [];

                console.log('Parsed Roles:', roles);

                const admin = roles.includes('Admin');
                const instructor = roles.includes('Instructor');

                setIsAdmin(admin);
                setCanCreate(admin || instructor);

                // Get User ID
                const userId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decoded['sub'];
                setCurrentUserId(userId);
            } catch (e) {
                console.error("Error decoding token", e);
            }
        }
    }, []);

    const loadCourses = async () => {
        try {
            console.log('Loading courses...');
            const params = new URLSearchParams({ page, pageSize: 5 });
            if (statusFilter) params.append('status', statusFilter);

            const res = await api.get(`/courses/search?${params.toString()}`);
            setCourses(res.data);
        } catch (e) {
            console.error('Error loading courses:', e);
            alert('Error al cargar cursos: ' + JSON.stringify(e.response?.data?.errors || e.message));
        }
    };

    useEffect(() => {
        loadCourses();
    }, [page, statusFilter]);

    const [error, setError] = useState('');

    const createCourse = async () => {
        setError('');
        if (!newTitle.trim()) {
            setError('El título del curso es obligatorio');
            return;
        }
        try {
            console.log('Creating course with title:', newTitle);
            const res = await api.post('/courses', { title: newTitle });
            console.log('Response:', res.data);
            if (res.data && res.data.id) {
                navigate(`/editor/${res.data.id}`);
            } else {
                setError('Error: No se recibió el ID del curso');
            }
        } catch (err) {
            console.error('Error creating course:', err);
            setError('Error al crear el curso: ' + (err.response?.data?.message || err.message));
        }
    };

    const logout = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <div className="container mt-5" style={{ maxWidth: '1200px', margin: '0 auto' }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h1>Panel de Cursos</h1>
                <button className="btn btn-outline-gold d-flex align-items-center gap-2" onClick={logout}>
                    <FaSignOutAlt /> Cerrar Sesión
                </button>
            </div>

            {canCreate && (
                <div className="premium-card p-4 mb-4">
                    <h5 className="mb-3">Crear Nuevo Curso</h5>
                    {error && <div className="alert alert-danger">{error}</div>}
                    <div className="input-group">
                        <input
                            className="form-control"
                            placeholder="Título del curso"
                            value={newTitle}
                            onChange={e => {
                                setNewTitle(e.target.value);
                                if (error) setError('');
                            }}
                        />
                        <button className="btn btn-gold px-4 d-flex align-items-center gap-2" onClick={createCourse}>
                            <FaPlus /> Crear Curso
                        </button>
                    </div>
                </div>
            )}

            <div className="premium-card p-4 mb-3">
                <div className="row align-items-center">
                    <div className="col-md-3">
                        <label className="form-label text-gold mb-0">Filtrar por Estado:</label>
                    </div>
                    <div className="col-md-4">
                        <select
                            className="form-control"
                            value={statusFilter}
                            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
                        >
                            <option value="">Todos</option>
                            <option value="Draft">Borrador</option>
                            <option value="Published">Publicado</option>
                        </select>
                    </div>
                </div>
            </div>

            <div className="premium-card p-4">
                <h5 className="mb-4">Mis Cursos</h5>
                <table className="table table-dark table-hover">
                    <thead>
                        <tr>
                            <th>Título</th>
                            <th>Estado</th>
                            <th>Lecciones</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>
                    <tbody>
                        {courses.map(c => (
                            <tr key={c.id}>
                                <td><strong>{c.title}</strong></td>
                                <td>
                                    <span className={`badge ${c.status === 'Published' ? 'badge-gold' : 'badge-dark'}`}>
                                        {c.status === 'Published' ? 'Publicado' : 'Borrador'}
                                    </span>
                                </td>
                                <td>
                                    <span className="badge badge-gold">{c.totalActiveLessons} lecciones</span>
                                </td>
                                <td>
                                    {(isAdmin || c.authorId === currentUserId) ? (
                                        <>
                                            <button
                                                className="btn btn-sm btn-gold me-2"
                                                onClick={() => navigate(`/editor/${c.id}`)}
                                                title="Editar"
                                            >
                                                <FaEdit />
                                            </button>
                                            <button
                                                className="btn btn-sm btn-outline-gold"
                                                onClick={async () => {
                                                    if (confirm('¿Eliminar este curso?')) {
                                                        await api.delete(`/courses/${c.id}`);
                                                        loadCourses();
                                                    }
                                                }}
                                                title="Eliminar"
                                            >
                                                <FaTrash />
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            className="btn btn-sm btn-gold"
                                            onClick={() => navigate(`/course/${c.id}`)}
                                            title="Ver Contenido"
                                        >
                                            <FaEye />
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <div className="d-flex justify-content-between align-items-center mt-4">
                    <button
                        className="btn btn-outline-gold d-flex align-items-center gap-2"
                        disabled={page <= 1}
                        onClick={() => setPage(page - 1)}
                    >
                        <FaChevronLeft /> Anterior
                    </button>
                    <span className="text-gold fw-bold">Página {page}</span>
                    <button
                        className="btn btn-outline-gold d-flex align-items-center gap-2"
                        onClick={() => setPage(page + 1)}
                    >
                        Siguiente <FaChevronRight />
                    </button>
                </div>
            </div>
        </div>
    );
}
