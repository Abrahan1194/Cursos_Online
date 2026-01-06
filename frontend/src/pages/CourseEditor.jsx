import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { jwtDecode } from 'jwt-decode';
import { FaArrowLeft, FaRocket, FaBan, FaSave, FaTimes, FaArrowUp, FaArrowDown, FaEdit, FaTrash, FaPlus } from 'react-icons/fa';

export default function CourseEditor() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [course, setCourse] = useState(null);
    const [newLessonTitle, setNewLessonTitle] = useState('');
    const [editingLesson, setEditingLesson] = useState(null);
    const [editTitle, setEditTitle] = useState('');
    const [editContent, setEditContent] = useState('');

    const [isAdmin, setIsAdmin] = useState(false);
    const [currentUserId, setCurrentUserId] = useState('');
    const [canEdit, setCanEdit] = useState(false);

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decoded = jwtDecode(token);
                console.log('Editor Token:', decoded);
                let roles = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                    decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] ||
                    decoded['role'];
                if (typeof roles === 'string') roles = [roles];
                if (!roles) roles = [];

                const admin = roles.includes('Admin');
                setIsAdmin(admin);
                setCurrentUserId(decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decoded['sub']);
            } catch (e) { }
        }
    }, []);

    const loadCourse = async () => {
        try {
            const res = await api.get(`/courses/${id}`);
            setCourse(res.data);
        } catch (err) {
            console.error(err);
        }
    };

    useEffect(() => {
        loadCourse();
    }, [id]);

    useEffect(() => {
        if (course && currentUserId) {
            setCanEdit(isAdmin || course.authorId === currentUserId);
        }
    }, [course, currentUserId, isAdmin]);

    const addLesson = async () => {
        if (!newLessonTitle) return;
        try {
            await api.post('/lessons', { courseId: id, title: newLessonTitle });
            setNewLessonTitle('');
            loadCourse();
        } catch (err) {
            alert('Error al crear la lección');
        }
    };

    const startEdit = (lesson) => {
        setEditingLesson(lesson.id);
        setEditTitle(lesson.title);
        setEditContent(lesson.content || '');
    };

    const saveEdit = async (lessonId) => {
        try {
            await api.put(`/lessons/${lessonId}`, { title: editTitle, content: editContent });
            setEditingLesson(null);
            loadCourse();
        } catch (err) {
            alert('Error al editar lección');
        }
    };

    const deleteLesson = async (lessonId) => {
        if (!confirm('¿Eliminar esta lección?')) return;
        try {
            await api.delete(`/lessons/${lessonId}`);
            loadCourse();
        } catch (err) {
            alert('Error al eliminar lección');
        }
    };

    const moveLesson = async (index, direction) => {
        const newIndex = direction === 'up' ? index - 1 : index + 1;
        if (newIndex < 0 || newIndex >= course.lessons.length) return;

        const newLessons = [...course.lessons];
        [newLessons[index], newLessons[newIndex]] = [newLessons[newIndex], newLessons[index]];

        try {
            await api.post('/lessons/reorder', {
                courseId: id,
                lessonIds: newLessons.map(l => l.id)
            });
            loadCourse();
        } catch (err) {
            alert('Error al reordenar lecciones');
        }
    };

    const publish = async () => {
        try {
            await api.patch(`/courses/${id}/publish`);
            loadCourse();
            alert('¡Curso publicado exitosamente!');
        } catch (err) {
            alert(err.response?.data?.message || 'Error al publicar el curso');
        }
    };

    if (!course) return (
        <div className="container mt-5 text-center">
            <div className="spinner-border text-gold"></div>
        </div>
    );

    return (
        <div className="container mt-5" style={{ maxWidth: '1000px', margin: '0 auto' }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h1>Editor de Curso</h1>
                <button className="btn btn-outline-gold d-flex align-items-center gap-2" onClick={() => navigate('/dashboard')}>
                    <FaArrowLeft /> Volver al Panel
                </button>
            </div>

            <div className="premium-card p-4 mb-4">
                <h3>{course.title}</h3>
                <p className="mb-0">
                    Estado: <span className={`badge ${course.status === 'Published' ? 'badge-gold' : 'badge-dark'}`}>
                        {course.status === 'Published' ? 'Publicado' : 'Borrador'}
                    </span>
                </p>
            </div>

            <div className="premium-card p-4 mb-4">
                <h5 className="mb-3">Lecciones del Curso</h5>
                {course.lessons && course.lessons.length > 0 ? (
                    <ul className="list-group mb-3">
                        {course.lessons.map((lesson, index) => (
                            <li key={lesson.id} className="list-group-item">
                                <div className="d-flex justify-content-between align-items-center">
                                    <div className="d-flex align-items-center flex-grow-1">
                                        <span className="badge badge-gold me-2">#{lesson.order}</span>
                                        {editingLesson === lesson.id ? (
                                            <div className="flex-grow-1 me-2">
                                                <input
                                                    className="form-control form-control-sm mb-2"
                                                    value={editTitle}
                                                    onChange={(e) => setEditTitle(e.target.value)}
                                                    placeholder="Título de la lección"
                                                />
                                                <textarea
                                                    className="form-control form-control-sm"
                                                    value={editContent}
                                                    onChange={(e) => setEditContent(e.target.value)}
                                                    placeholder="Contenido de la lección (Texto, HTML, etc.)"
                                                    rows="4"
                                                />
                                            </div>
                                        ) : (
                                            <strong>{lesson.title}</strong>
                                        )}
                                    </div>
                                    <div className="d-flex gap-1">
                                        {editingLesson === lesson.id ? (
                                            <>
                                                <button
                                                    className="btn btn-sm btn-gold"
                                                    onClick={() => saveEdit(lesson.id)}
                                                    title="Guardar"
                                                >
                                                    <FaSave />
                                                </button>
                                                <button
                                                    className="btn btn-sm btn-outline-gold"
                                                    onClick={() => setEditingLesson(null)}
                                                    title="Cancelar"
                                                >
                                                    <FaTimes />
                                                </button>
                                            </>
                                        ) : (
                                            canEdit && (
                                                <>
                                                    <button
                                                        className="btn btn-sm btn-outline-gold"
                                                        onClick={() => moveLesson(index, 'up')}
                                                        disabled={index === 0}
                                                        title="Subir"
                                                    >
                                                        <FaArrowUp />
                                                    </button>
                                                    <button
                                                        className="btn btn-sm btn-outline-gold"
                                                        onClick={() => moveLesson(index, 'down')}
                                                        disabled={index === course.lessons.length - 1}
                                                        title="Bajar"
                                                    >
                                                        <FaArrowDown />
                                                    </button>
                                                    <button
                                                        className="btn btn-sm btn-gold"
                                                        onClick={() => startEdit(lesson)}
                                                        title="Editar"
                                                    >
                                                        <FaEdit />
                                                    </button>
                                                    <button
                                                        className="btn btn-sm btn-outline-gold"
                                                        onClick={() => deleteLesson(lesson.id)}
                                                        title="Eliminar"
                                                    >
                                                        <FaTrash />
                                                    </button>
                                                </>
                                            )
                                        )}
                                    </div>
                                </div>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="text-muted">No hay lecciones aún. Agrega la primera.</p>
                )}

                {canEdit && (
                    <div className="input-group">
                        <input
                            className="form-control"
                            placeholder="Título de la nueva lección"
                            value={newLessonTitle}
                            onChange={e => setNewLessonTitle(e.target.value)}
                        />
                        <button className="btn btn-gold px-4 d-flex align-items-center gap-2" onClick={addLesson}>
                            <FaPlus /> Agregar
                        </button>
                    </div>
                )}
            </div>

            {canEdit && (
                <div className="mt-4 text-center">
                    {course.status === 'Draft' ? (
                        <button className="btn btn-gold btn-lg px-5 d-flex align-items-center gap-2 mx-auto" onClick={publish}>
                            <FaRocket /> Publicar Curso
                        </button>
                    ) : (
                        <button className="btn btn-outline-gold btn-lg px-5 d-flex align-items-center gap-2 mx-auto" onClick={async () => {
                            await api.patch(`/courses/${id}/unpublish`);
                            loadCourse();
                            alert('Curso despublicado');
                        }}>
                            <FaBan /> Despublicar Curso
                        </button>
                    )}
                </div>
            )}
        </div>
    );
}
