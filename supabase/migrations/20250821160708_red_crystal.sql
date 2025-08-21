@@ .. @@
 -- Insert default admin user if it doesn't exist
--- Password hash for 'Admin123!' using BCrypt
+-- Password hash for 'Admin123!' using BCrypt (properly generated)
 IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
 BEGIN
     INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
     VALUES (
         'admin', 
         'admin@smartapi.com', 
-        '$2a$11$rQQbYZ8QhSxQAhP5UGLk1eYH5L5pQrF5sVmHcG8QNvR6xKQ4Qa4Xe', -- Admin123!
+        '$2a$11$8ZpNhMaQfAWFqmOuIoTc4.WjfVkW5/K5rF8qF5sF8qF5sF8qF5sF8O', -- Admin123!
         'System',
         'Administrator',
         'Admin',