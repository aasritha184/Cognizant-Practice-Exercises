CREATE DATABASE IF NOT EXISTS event_portal;
USE event_portal;

CREATE TABLE Users (
  user_id INT PRIMARY KEY AUTO_INCREMENT,
  full_name VARCHAR(100) NOT NULL,
  email VARCHAR(100) UNIQUE NOT NULL,
  city VARCHAR(100) NOT NULL,
  registration_date DATE NOT NULL
);

CREATE TABLE Events (
  event_id INT PRIMARY KEY AUTO_INCREMENT,
  title VARCHAR(200) NOT NULL,
  description TEXT,
  city VARCHAR(100) NOT NULL,
  start_date DATETIME NOT NULL,
  end_date DATETIME NOT NULL,
  status ENUM('upcoming','completed','cancelled') NOT NULL,
  organizer_id INT,
  FOREIGN KEY (organizer_id) REFERENCES Users(user_id)
);

CREATE TABLE Sessions (
  session_id INT PRIMARY KEY AUTO_INCREMENT,
  event_id INT NOT NULL,
  title VARCHAR(200) NOT NULL,
  speaker_name VARCHAR(100) NOT NULL,
  start_time DATETIME NOT NULL,
  end_time DATETIME NOT NULL,
  FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

CREATE TABLE Registrations (
  registration_id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT NOT NULL,
  event_id INT NOT NULL,
  registration_date DATE NOT NULL,
  FOREIGN KEY (user_id) REFERENCES Users(user_id),
  FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

CREATE TABLE Feedback (
  feedback_id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT NOT NULL,
  event_id INT NOT NULL,
  rating INT CHECK (rating BETWEEN 1 AND 5),
  comments TEXT,
  feedback_date DATE NOT NULL,
  FOREIGN KEY (user_id) REFERENCES Users(user_id),
  FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

CREATE TABLE Resources (
  resource_id INT PRIMARY KEY AUTO_INCREMENT,
  event_id INT NOT NULL,
  resource_type ENUM('pdf','image','link') NOT NULL,
  resource_url VARCHAR(255) NOT NULL,
  uploaded_at DATETIME NOT NULL,
  FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

INSERT INTO Users (full_name, email, city, registration_date) VALUES
('Alice Johnson', 'alice@example.com', 'New York', '2024-12-01'),
('Bob Smith', 'bob@example.com', 'Los Angeles', '2024-12-05'),
('Charlie Lee', 'charlie@example.com', 'Chicago', '2024-12-10'),
('Diana King', 'diana@example.com', 'New York', '2025-01-15'),
('Ethan Hunt', 'ethan@example.com', 'Los Angeles', '2025-02-01');

INSERT INTO Events (title, description, city, start_date, end_date, status, organizer_id) VALUES
('Tech Innovators Meetup', 'A meetup for tech enthusiasts.', 'New York', '2025-06-10 10:00:00', '2025-06-10 16:00:00', 'upcoming', 1),
('AI & ML Conference', 'Conference on AI and ML advancements.', 'Chicago', '2025-05-15 09:00:00', '2025-05-15 17:00:00', 'completed', 3),
('Frontend Development Bootcamp', 'Hands-on training on frontend tech.', 'Los Angeles', '2025-07-01 10:00:00', '2025-07-03 16:00:00', 'upcoming', 2);

INSERT INTO Sessions (event_id, title, speaker_name, start_time, end_time) VALUES
(1, 'Opening Keynote', 'Dr. Tech', '2025-06-10 10:00:00', '2025-06-10 11:00:00'),
(1, 'Future of Web Dev', 'Alice Johnson', '2025-06-10 11:15:00', '2025-06-10 12:30:00'),
(2, 'AI in Healthcare', 'Charlie Lee', '2025-05-15 09:30:00', '2025-05-15 11:00:00'),
(3, 'Intro to HTML5', 'Bob Smith', '2025-07-01 10:00:00', '2025-07-01 12:00:00');

INSERT INTO Registrations (user_id, event_id, registration_date) VALUES
(1, 1, '2025-05-01'),
(2, 1, '2025-05-02'),
(3, 2, '2025-04-30'),
(4, 2, '2025-04-28'),
(5, 3, '2025-06-15');

INSERT INTO Feedback (user_id, event_id, rating, comments, feedback_date) VALUES
(3, 2, 4, 'Great insights!', '2025-05-16'),
(4, 2, 5, 'Very informative.', '2025-05-16'),
(2, 1, 3, 'Could be better.', '2025-06-11');

INSERT INTO Resources (event_id, resource_type, resource_url, uploaded_at) VALUES
(1, 'pdf', 'https://portal.com/resources/tech_meetup_agenda.pdf', '2025-05-01 10:00:00'),
(2, 'image', 'https://portal.com/resources/ai_poster.jpg', '2025-04-20 09:00:00'),
(3, 'link', 'https://portal.com/resources/html5_docs', '2025-06-25 15:00:00');

-- 1. upcoming events for a user, same city, sorted by date
SELECT e.title, e.start_date, e.city
FROM Events e
JOIN Registrations r ON r.event_id = e.event_id
JOIN Users u ON u.user_id = r.user_id
WHERE e.status = 'upcoming' AND e.city = u.city
ORDER BY e.start_date;


-- 2. top rated events (need 10+ reviews to count)
SELECT e.event_id, e.title, AVG(f.rating) AS avg_rating, COUNT(f.feedback_id) AS feedback_count
FROM Events e
JOIN Feedback f ON f.event_id = e.event_id
GROUP BY e.event_id, e.title
HAVING COUNT(f.feedback_id) >= 10
ORDER BY avg_rating DESC;


-- 3. users who haven't registered for anything in 90 days
SELECT u.*
FROM Users u
WHERE u.user_id NOT IN (
  SELECT r.user_id FROM Registrations r
  WHERE r.registration_date >= CURDATE() - INTERVAL 90 DAY
);


-- 4. sessions happening 10am-12pm per event
SELECT event_id, COUNT(*) AS session_count
FROM Sessions
WHERE TIME(start_time) >= '10:00:00' AND TIME(start_time) < '12:00:00'
GROUP BY event_id;


-- 5. top 5 cities by distinct registrations
SELECT e.city, COUNT(DISTINCT r.user_id) AS distinct_registrations
FROM Events e
JOIN Registrations r ON r.event_id = e.event_id
GROUP BY e.city
ORDER BY distinct_registrations DESC
LIMIT 5;


-- 6. resource counts per event by type
SELECT e.event_id, e.title,
  SUM(res.resource_type = 'pdf') AS pdf_count,
  SUM(res.resource_type = 'image') AS image_count,
  SUM(res.resource_type = 'link') AS link_count
FROM Events e
LEFT JOIN Resources res ON res.event_id = e.event_id
GROUP BY e.event_id, e.title;


-- 7. low ratings (below 3) with comments
SELECT u.full_name, f.rating, f.comments, e.title
FROM Feedback f
JOIN Users u ON u.user_id = f.user_id
JOIN Events e ON e.event_id = f.event_id
WHERE f.rating < 3;


-- 8. session counts for upcoming events only
SELECT e.event_id, e.title, COUNT(s.session_id) AS session_count
FROM Events e
LEFT JOIN Sessions s ON s.event_id = e.event_id
WHERE e.status = 'upcoming'
GROUP BY e.event_id, e.title;


-- 9. events per organizer + status
SELECT o.user_id, o.full_name, COUNT(e.event_id) AS total_events, e.status
FROM Users o
JOIN Events e ON e.organizer_id = o.user_id
GROUP BY o.user_id, o.full_name, e.status;


-- 10. events that got registrations but zero feedback
SELECT e.event_id, e.title
FROM Events e
JOIN Registrations r ON r.event_id = e.event_id
LEFT JOIN Feedback f ON f.event_id = e.event_id AND f.user_id = r.user_id
WHERE f.feedback_id IS NULL
GROUP BY e.event_id, e.title;


-- 11. new users per day, last 7 days
SELECT registration_date, COUNT(*) AS new_users
FROM Users
WHERE registration_date >= CURDATE() - INTERVAL 7 DAY
GROUP BY registration_date
ORDER BY registration_date;


-- 12. event(s) with the most sessions
SELECT event_id, COUNT(*) AS session_count
FROM Sessions
GROUP BY event_id
ORDER BY session_count DESC
LIMIT 1;


-- 13. avg rating grouped by city
SELECT e.city, AVG(f.rating) AS avg_rating
FROM Events e
JOIN Feedback f ON f.event_id = e.event_id
GROUP BY e.city;


-- 14. top 3 most registered events
SELECT e.event_id, e.title, COUNT(r.registration_id) AS total_registrations
FROM Events e
JOIN Registrations r ON r.event_id = e.event_id
GROUP BY e.event_id, e.title
ORDER BY total_registrations DESC
LIMIT 3;


-- 15. overlapping session times within the same event
SELECT s1.session_id AS session_a, s2.session_id AS session_b, s1.event_id
FROM Sessions s1
JOIN Sessions s2 ON s1.event_id = s2.event_id
  AND s1.session_id < s2.session_id
  AND s1.start_time < s2.end_time
  AND s2.start_time < s1.end_time;


-- 16. new accounts (30 days) with no registrations yet
SELECT u.*
FROM Users u
WHERE u.registration_date >= CURDATE() - INTERVAL 30 DAY
  AND u.user_id NOT IN (SELECT user_id FROM Registrations);


-- 17. speakers doing more than one session
SELECT speaker_name, COUNT(*) AS session_count
FROM Sessions
GROUP BY speaker_name
HAVING COUNT(*) > 1;


-- 18. events with zero resources uploaded
SELECT e.event_id, e.title
FROM Events e
LEFT JOIN Resources res ON res.event_id = e.event_id
WHERE res.resource_id IS NULL;


-- 19. completed events - registrations + avg rating
SELECT e.event_id, e.title,
  COUNT(DISTINCT r.registration_id) AS total_registrations,
  AVG(f.rating) AS avg_rating
FROM Events e
LEFT JOIN Registrations r ON r.event_id = e.event_id
LEFT JOIN Feedback f ON f.event_id = e.event_id
WHERE e.status = 'completed'
GROUP BY e.event_id, e.title;


-- 20. per user: events attended + feedback given
SELECT u.user_id, u.full_name,
  COUNT(DISTINCT r.event_id) AS events_attended,
  COUNT(DISTINCT f.feedback_id) AS feedback_submitted
FROM Users u
LEFT JOIN Registrations r ON r.user_id = u.user_id
LEFT JOIN Feedback f ON f.user_id = u.user_id
GROUP BY u.user_id, u.full_name;


-- 21. top 5 users by feedback count
SELECT u.user_id, u.full_name, COUNT(f.feedback_id) AS feedback_count
FROM Users u
JOIN Feedback f ON f.user_id = u.user_id
GROUP BY u.user_id, u.full_name
ORDER BY feedback_count DESC
LIMIT 5;


-- 22. check for duplicate registrations (same user+event twice)
SELECT user_id, event_id, COUNT(*) AS registration_count
FROM Registrations
GROUP BY user_id, event_id
HAVING COUNT(*) > 1;


-- 23. month by month registration trend, last 12 months
SELECT DATE_FORMAT(registration_date, '%Y-%m') AS month, COUNT(*) AS registrations
FROM Registrations
WHERE registration_date >= CURDATE() - INTERVAL 12 MONTH
GROUP BY month
ORDER BY month;


-- 24. avg session length per event in minutes
SELECT event_id, AVG(TIMESTAMPDIFF(MINUTE, start_time, end_time)) AS avg_duration_minutes
FROM Sessions
GROUP BY event_id;


-- 25. events with no sessions scheduled
SELECT e.event_id, e.title
FROM Events e
LEFT JOIN Sessions s ON s.event_id = e.event_id
WHERE s.session_id IS NULL;
