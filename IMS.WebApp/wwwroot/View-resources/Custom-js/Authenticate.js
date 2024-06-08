$(function () {
    // Handle form submission
    
    $('#loginForm').on('submit', function (event) {
        event.preventDefault(); // Prevent default form submission
        var $submitButton = $('#loginButton');
        if ($submitButton.data('submitted') === true) {
            return; // Prevent multiple submissions
        }
        $submitButton.data('submitted', true);
        $submitButton.prop('disabled', true);

        let formData = $(this).serialize();
        // Send AJAX request
        $.ajax({
            type: 'POST',
            url: '/Login',
            data: formData,
            success: function (response) {
                toastr.success('Login successfully!');
                setTimeout(function () {
                    $submitButton.data('submitted', false);
                    $submitButton.prop('disabled', false);
                    window.location.href = '/Employees';
                }, 1000); 
            },
            error: function (xhr, status, error) {
                $submitButton.data('submitted', false);
                $submitButton.prop('disabled', false);
                toastr.error('Incorrect username or password!');
            }
        });
    });
});
