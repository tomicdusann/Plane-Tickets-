$(document).ready(function () {
    console.log('Script loaded and ready');

    $('#loginForm').on('submit', function (e) {
        e.preventDefault();

        var loginData = {
            UserName: $('#loginUserName').val().trim(),
            Password: $('#loginPassword').val().trim()
        };

        if (!loginData.UserName || !loginData.Password) {
            $('#message').text('Please enter both username and password.');
            return;
        }

        try {
            var jsonData = JSON.stringify(loginData);
            console.log('U traju smo');
            $.ajax({
                url: 'http://localhost:62378/api/Users/Login',
                type: 'POST',
                contentType: 'application/json',
                data: jsonData,
                success: function (response) {
                    $('#message').text('Login successful! Welcome, ' + response.UserName);
                    console.log('Usli smo u success');
                    localStorage.setItem('token', response.Token);
                    // Predefinisani admin podaci
                    var adminCredentials = [
                        { UserName: "admin1", Password: "password1" },
                        { UserName: "admin2", Password: "password2" },
                        { UserName: "admin3", Password: "password3" }
                    ];

                    // Provera da li su podaci za prijavu među admin podacima
                    var isAdmin = adminCredentials.some(function (ac) {
                        return ac.UserName === response.UserName && ac.Password === loginData.Password;
                    });

                    if (isAdmin) {
                        window.location.href = 'Html pages/AdminControlHub.html';
                    } else {
                        // Redirect to user dashboard or another appropriate page
                        window.location.href = 'UserControlHub.html'; // Replace with the actual user dashboard page
                    }
                    // Reset the form or redirect to another page if needed
                    //window.location.href = 'Html pages/AdminControlHub.html';
                },
                error: function (xhr, status, error) {
                    try {
                        var err = JSON.parse(xhr.responseText);
                        $('#message').text('Login failed: ' + err.Message);
                    } catch (e) {
                        $('#message').text('Login failed: An unknown error occurred.');
                    }
                }
            });
        } catch (e) {
            $('#message').text('Invalid data format.');
        }
    });

    $('#registerForm').on('submit', function (e) {
        e.preventDefault();

        var registerData = {
            UserName: $('#registerUserName').val().trim(),
            Password: $('#registerPassword').val().trim(),
            Name: $('#registerName').val().trim(),
            Surname: $('#registerSurname').val().trim(),
            Email: $('#registerEmail').val().trim(),
            DateOfBirth: $('#registerDateOfBirth').val().trim(),
            Gender: $('#registerGender').val(),
            TypeOfUser: $('#registerTypeOfUser').val()
        };

        if (!registerData.UserName || !registerData.Password || !registerData.Email) {
            $('#message').text('Please fill in all required fields.');
            return;
        }

        try { 
            var jsonData = JSON.stringify(registerData);
            console.log('Prepared JSON data for registration:', jsonData); // Logging before AJAX call
            $.ajax({
                url: 'http://localhost:62378/api/Users/Register', // Ensure this is the correct URL
                type: 'POST',
                contentType: 'application/json',
                data: jsonData,
                beforeSend: function () {
                    console.log('Before sending the request');
                },
                success: function (response) {
                    console.log('In success callback:', response);
                    $('#message').text('Registration successful! Welcome, ' + response.UserName);
                    // Reset the form or redirect to another page if needed
                },
                error: function (xhr, status, error) {
                    console.log('In error callback: Status:', status, 'Error:', error);
                    try {
                        var err = JSON.parse(xhr.responseText);
                        $('#message').text('Registration failed: ' + err.Message);
                    } catch (e) {
                        $('#message').text('Registration failed: An unknown error occurred.');
                    }
                }
            });
        } catch (e) {
            console.log('In error callback: Status:', status, 'Error:', error);
            $('#message').text('Invalid data format.');
        }
    });
});
