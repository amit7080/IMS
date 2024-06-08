$(function () {

    ApplyFilters();
    let _$employeeTable = $('#employeesTable').DataTable({
        paging: true,
        serverSide: true,
        searching: false,
        ajax: function (data, callback, settings) {
            let filter = $('#EmployeeAdvanceSearchForm').serializeFormToObject(true);
            filter.maxResultCount = data.length;
            filter.skipCount = data.start;
            filter.Search = $('#SearchInput').val();
            filter.startDate = $('#startDateEmployee').val();
            filter.endDate = $('#endDateEmployee').val();

            $.ajax({
                url: '/GetAllUsers',
                type: 'GET',
                data: filter,
                success: function (result) {

                    callback({
                        recordsTotal: result.recordsTotal,
                        recordsFiltered: result.recordsTotal,
                        data: result.data
                    });
                },
                complete: function () {
                }
            });
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () {
                    _$employeeTable.draw(false);
                }
            }
        ],
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: '_all',
                className: 'text-center', // Apply text-center to all columns
            },
            {
                targets: 0,
                data: 'firstName',
                sortable: false,
                autoWidth: true,
                render: function (data, type, row) {
                    return row.firstName + ' ' + row.lastName;
                }
            },
            {
                targets: 1,
                data: 'joiningDate',
                sortable: false,
                autoWidth: true,
                render: function (data) {
                    if (data) {
                        let dateWithoutTime = data.split('T')[0];
                        let dateParts = dateWithoutTime.split('-');
                        let formattedDate = new Date(Date.UTC(
                            parseInt(dateParts[0], 10),
                            parseInt(dateParts[1], 10) - 1,
                            parseInt(dateParts[2], 10),
                            0, 0, 0, 0
                        ));

                        let localDate = new Date(formattedDate.getTime() - (formattedDate.getTimezoneOffset() * 60000));

                        return localDate.toISOString().split('T')[0];
                    } else {
                        return '';
                    }
                }
            },
            {
                targets: 2,
                data: 'phoneNumber',
                sortable: false,
                autoWidth: true,
            },
            {
                targets: 3,
                data: 'department.name',
                sortable: false,
                autoWidth: true,
            },
            {
                targets: 4,
                data: 'profileImage',
                sortable: false,
                autoWidth: true,
                render: function (data, type) {
                    return `<img src= '/UserImages/${data}'  height="50px" width="50px"/>`;
                }
            }, {
                targets: 5,
                data: 'roles[0]',
                sortable: false,
                autoWidth: true,
            },
            {
                targets: 6,
                data: 'isActive',
                sortable: false,
                autoWidth: true,
                render: function (data) {

                    if (data) {
                        return '<i class="fas fa-check-circle text-success"></i>';
                    } else {
                        return '<i class="fas fa-times-circle text-danger"></i>';
                    }
                }
            },
            {
                targets: 7,
                data: null,
                sortable: false,
                autoWidth: true,
                defaultContent: '',
                render: function (data, type, row, meta) {

                    if (row.isActive) {
                        return [
                            `<button type="button" class="btn btn-sm bg-danger employee-Status" data-action="deactivated" data-user-id="${row.id}" ">`,
                            'Deactivate User',
                            '</button>'
                        ].join('');
                    } else {
                        return [
                            `<button type="button" class="btn btn-sm bg-danger employee-Status" data-action="activated" data-user-id="${row.id}" ">`,
                            'Activate User',
                            '</button>'
                        ].join('');
                    }
                }
            }
        ]
    });


    $(document).on('click', '.employee-Status', function () {
        let userID = $(this).data("user-id");
        let dataAction = $(this).data("action");
        // Confirm deletion with the user
        Swal.fire({
            title: `Are you sure you want to ${dataAction} this user?`,
            //text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: `Yes, ${dataAction} it!`
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/deactivateUser', // Service endpoint URL
                    type: 'POST', // HTTP method
                    data: {
                        userId: userID,
                    },
                    success: function () {
                        // Notify the user of success
                        toastr.success(`Employee ${dataAction} successfully`);
                        _$employeeTable.ajax.reload();
                    },
                    error: function (xhr, status, error) {
                        toastr.error(`Failed to ${dataAction} employee`);
                    },
                });
            }
        });
    });

    $('#openCreateEmployeeModalBtn').on('click', function () {

        $.ajax({
            url: "/GetAllDepartments", // Assuming this is the correct endpoint URL
            method: "GET",
            success: function (result) {

                let jsonData1 = result;
                let appenddata1 = '<option value="">Select Department</option>';
                $("#departmentsName").empty();
                for (const element of jsonData1) {
                    appenddata1 += "<option value='" + element.departmentId + "'>" + element.name + "</option>";
                }
                $("#departmentsName").append(appenddata1);
            },
            error: function (xhr, status, error) {
                console.error("Error fetching departments:", error);
                // Handle error here, such as displaying an error message to the user
            }
        });
        $.ajax({
            url: "/GetAllManager", // Assuming this is the correct endpoint URL
            method: "GET",
            success: function (result) {

                let jsonData1 = result;
                let appenddata1 = '<option value="">Select Manager</option>';
                $("#assignedManagerId").empty();
                for (const element of jsonData1) {
                    appenddata1 += "<option value='" + element.userId + "'>" + element.userName + "</option>";
                }
                $("#assignedManagerId").append(appenddata1);
            },
            error: function (xhr, status, error) {
                console.error("Error fetching departments:", error);
                // Handle error here, such as displaying an error message to the user
            }
        });
        $.ajax({
            url: "/GetAllHR", // Assuming this is the correct endpoint URL
            method: "GET",
            success: function (result) {

                let jsonData1 = result;
                let appenddata1 = '<option value="">Select HR</option>';
                $("#assignedHrId").empty();
                for (const element of jsonData1) {
                    appenddata1 += "<option value='" + element.userId + "'>" + element.userName + "</option>";
                }
                $("#assignedHrId").append(appenddata1);
            },
            error: function (xhr, status, error) {
                console.error("Error fetching departments:", error);
                // Handle error here, such as displaying an error message to the user
            }
        });

        $.ajax({
            url: "/GetAllRoles", // Assuming this is the correct endpoint URL
            method: "GET",
            success: function (result) {

                let jsonData1 = result;
                let appenddata1 = '<option value="">Select Roles</option>';
                $("#roles").empty();
                for (const element of jsonData1) {
                    appenddata1 += "<option value='" + element.id + "'>" + element.name + "</option>";
                }
                $("#roles").append(appenddata1);
            },
            error: function (xhr, status, error) {
                console.error("Error fetching departments:", error);
                // Handle error here, such as displaying an error message to the user
            }
        });
        // Show the modal when the partial view is loaded
        $('#CreateEmployeeModal').modal('show');
    });

    $('.closeBtnEmployee').on('click', function () {
        $('#CreateEmployeeModal').modal('hide');
    });
    $('.closeBtnEmployee').on('click', function () {
        $('#createEmployee')[0].reset();
        $('#CreateEmployeeModal').modal('hide');
    });

    $('.btn-search').on('click', (e) => {
        _$employeeTable.ajax.reload();
    });

    $('#filterButtonEmployee').on('click', (e) => {

        _$employeeTable.ajax.reload();
    });

    $('#clearfilterButtonEmployee').on('click', (e) => {
        $('#startDateEmployee').val('');
        $('#endDateEmployee').val('');
        $('#errorMessageEmployee').text("");
        ApplyFilters();
        _$employeeTable.ajax.reload();
    });


    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$employeeTable.ajax.reload();
            return false;
        }
    });

    $('#createEmployeeForm').on('submit', function (event) {

        event.preventDefault();

        // Collect form data
        var formData = new FormData(this);
        var fileInput = $('#fileInput')[0].files[0];
        var reader = new FileReader();

        reader.onloadend = function () {
            var base64String = reader.result.split(',')[1];
            formData.set('profileImage', base64String);
            var user = {
                FirstName: formData.get('firstName'),
                LastName: formData.get('lastName'),
                Email: formData.get('emailAddress'),
                Phone: formData.get('phoneNumber'),
                Password: formData.get('password'),
                Address: formData.get('Address'),
                Gender: formData.get('gender'),
                Role: formData.get('role'),
                DOB: formData.get('dOB'),
                JoiningDate: formData.get('joiningDate'),
                DepartmentId: formData.get('departmentId'),
                AssignedManagerId: formData.get('assignedManagerId'),
                AssignedHrId: formData.get('assignedHrId'),
                ProfileImage: base64String,
                ImageName: fileInput.name
            };

            CreateEmployee(user);
        };

        if (fileInput) {
            reader.readAsDataURL(fileInput);
        } else {
            alert('Please select a profile image.');
        }
    });

    function CreateEmployee(user) {

        $.ajax({
            url: '/CreateEmployee',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(user),
            success: function (response) {
                if (response) {
                    toastr.success('Employee added successfully');
                    _$employeeTable.ajax.reload();

                    $('#CreateEmployeeModal').modal('hide');

                }
            },
            error: function (xhr, status, error) {
                toastr.error('Failed to create employee');
                // Handle error, show error message
            }
        });
    }

});


function ApplyFilters() {
    // Initialize elements and current date
    let startDateInputEmployee = $('#startDateEmployee');
    let endDateInputEmployee = $('#endDateEmployee');
    let filterButtonEmployee = $('#filterButtonEmployee');
    let clearFilterButtonEmployee = $('#clearfilterButtonEmployee');
    let errorMessageDivForEmployee = $('#errorMessageEmployee');
    let currentDate = new Date();
    currentDate.setHours(0, 0, 0, 0);

    // Disable buttons initially
    filterButtonEmployee.addClass('btn bg-blue').prop('disabled', true);
    clearFilterButtonEmployee.addClass('btn bg-blue').prop('disabled', true);

    function validateDates() {
        let startDate = new Date(startDateInputEmployee.val());
        startDate.setHours(0, 0, 0, 0);
        let endDate = new Date(endDateInputEmployee.val());
        endDate.setHours(0, 0, 0, 0);

        // Initialize error message
        let errorMessage = '';

        // Validate start date
        if (startDateInputEmployee.val() && startDate > currentDate) {
            errorMessage = "Start date cannot be in the future.";
        }
        // Validate end date
        else if (endDateInputEmployee.val() && endDate > currentDate) {
            errorMessage = "End date cannot be in the future.";
        }
        // Validate start date is not after end date
        else if (startDateInputEmployee.val() && endDateInputEmployee.val() && startDate > endDate) {
            errorMessage = "Start date cannot be after end date.";
        }

        // Display error message or enable buttons
        if (errorMessage) {
            filterButtonEmployee.prop('disabled', true);
            errorMessageDivForEmployee.text(errorMessage).css('color', 'red');
        } else {
            errorMessageDivForEmployee.text("");
            filterButtonEmployee.prop('disabled', false);
            clearFilterButtonEmployee.prop('disabled', false);
        }
    }

    // Attach input event listeners
    startDateInputEmployee.on('input', validateDates);
    endDateInputEmployee.on('input', validateDates);
}

