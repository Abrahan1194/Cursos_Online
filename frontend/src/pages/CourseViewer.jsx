import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { FaArrowLeft, FaBookOpen } from 'react-icons/fa';

export default function CourseViewer() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [course, setCourse] = useState(null);
    const [selectedLesson, setSelectedLesson] = useState(null);

    useEffect(() => {
        const loadCourse = async () => {
            try {
                const res = await api.get(`/courses/${id}`);
                setCourse(res.data);
                if (res.data.lessons && res.data.lessons.length > 0) {
                    setSelectedLesson(res.data.lessons[0]);
                }
            } catch (err) {
                console.error(err);
                alert('Error al cargar el curso.');
                navigate('/dashboard');
            }
        };
        loadCourse();
    }, [id, navigate]);

    if (!course) return (
        <div className="container mt-5 text-center">
            <div className="spinner-border text-gold"></div>
        </div>
    );

    return (
        <div className="container-fluid min-vh-100 bg-dark text-light p-0">
            {/* Header */}
            <div className="bg-secondary p-3 d-flex align-items-center shadow-sm">
                <button className="btn btn-outline-gold me-3" onClick={() => navigate('/dashboard')}>
                    <FaArrowLeft /> Volver
                </button>
                <h4 className="m-0 text-white flex-grow-1">{course.title}</h4>
                <div className="text-gold">
                    <FaBookOpen className="me-2" /> Modo Aprendizaje
                </div>
            </div>

            <div className="d-flex" style={{ height: 'calc(100vh - 70px)' }}>
                {/* Sidebar - Lessons List */}
                <div className="bg-dark border-end border-secondary p-0" style={{ width: '300px', overflowY: 'auto' }}>
                    <div className="p-3 bg-secondary border-bottom border-secondary">
                        <h6 className="m-0 text-gold">Contenido del Curso</h6>
                    </div>
                    <ul className="list-group list-group-flush">
                        {course.lessons.map((lesson, index) => (
                            <li
                                key={lesson.id}
                                className={`list-group-item bg-dark text-light border-secondary p-3 ${selectedLesson?.id === lesson.id ? 'active-lesson' : ''}`}
                                style={{ cursor: 'pointer', borderLeft: selectedLesson?.id === lesson.id ? '4px solid #FFD700' : 'none' }}
                                onClick={() => setSelectedLesson(lesson)}
                            >
                                <div className="d-flex align-items-center">
                                    <span className="badge bg-secondary text-gold me-2">#{lesson.order}</span>
                                    <span>{lesson.title}</span>
                                </div>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* Main Content - Lesson Body */}
                <div className="flex-grow-1 p-5 bg-black" style={{ overflowY: 'auto' }}>
                    {selectedLesson ? (
                        <div className="premium-card p-5" style={{ maxWidth: '900px', margin: '0 auto' }}>
                            <h2 className="text-gold mb-4">{selectedLesson.title}</h2>
                            <hr className="border-secondary mb-4" />
                            <div className="lesson-content">
                                {selectedLesson.content ? (
                                    <div style={{ whiteSpace: 'pre-wrap', lineHeight: '1.6', fontSize: '1.1rem' }}>
                                        {selectedLesson.content}
                                    </div>
                                ) : (
                                    <p className="text-muted fst-italic">Esta lección aún no tiene contenido.</p>
                                )}
                            </div>
                        </div>
                    ) : (
                        <div className="text-center mt-5 text-muted">
                            <FaBookOpen size={50} className="mb-3" />
                            <h3>Selecciona una lección para comenzar</h3>
                        </div>
                    )}
                </div>
            </div>

            <style>{`
                .active-lesson {
                    background-color: #2c2c2c !important;
                }
                .list-group-item:hover {
                    background-color: #2c2c2c;
                }
            `}</style>
        </div>
    );
}
