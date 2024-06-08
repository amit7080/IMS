$(function () {
    // Handle form submission

    ApplyFilters();
    let _$departmentsTable = $('#departmentsTable').DataTable({
        paging: true,
        serverSide: true,
        searching: false,
        response:true,
        ajax: function (data, callback, settings) {
            let filter = $('#departmentAdvanceSearchForm').serializeFormToObject(true);
            filter.maxResultCount = data.length;
            filter.skipCount = data.start;
            filter.Search = $('#SearchInput').val();
            filter.startDate = $('#startDateDepartment').val();
            filter.endDate = $('#endDateDepartment').val();

            $.ajax({
                url: '/FetchDepartments',
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
                    _$departmentsTable.ajax.reload(); // Reload the DataTable
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
                className: 'text-center', 
            },
            {
                targets: 0,
                data: 'name',
                sortable: false,
            },
            {
                targets: 1,
                data: 'creationDate',
                sortable: false,
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
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-department mt-2" data-department-id="${row.departmentId}" >`,
                        ' <i class="ri-edit-box-fill"></i>',
                        '   </button>',
                        `<button type="button" class="btn btn-sm bg-danger delete-department  ms-2 mt-2" data-department-id="${row.departmentId}" data-department-name="${row.departmentName}">`,
                        '<i class="ri-delete-bin-5-line"></i>',
                        '</button>'
                    ].join('');
                }
            }
        ]
    });

    $(document).on('click', '.edit-department', function () {
        // Perform AJAX call to fetch department data
        $.ajax({
            url: '/GetDepartmentById', // Assuming the route is correctly configured
            type: 'POST',
            dataType: 'json',
            data: {
                id: $(this).data('department-id'),
            },
            success: function (response) {
                // Populate the modal fields with the fetched department data
                $('#editDepartmentName').val(response.name); // Assuming there's an input field with ID 'editDepartmentName'
                $('#departmentId').val(response.departmentId); // Assuming there's an input field with ID 'editDepartmentName'
                // Show the edit modal
                $('#EditDepartmentModal').modal('show');
            },
            error: function (xhr, status, error) {
                // Handle error if any
                console.error(error);
            }
        });
    });

    $('.closeEditBtnDepartment').on('click', function () {
        $('#EditDepartmentModal').modal('hide');
    });

    $('#editDepartmentForm').on('submit', function (event) {
        event.preventDefault();

        // Perform AJAX call to update the department
        $.ajax({
            url: '/EditDepartment',
            type: 'POST',
            data: $('#editDepartmentForm').serialize(),
            success: function (response) {
                toastr.success('Department updated successfully!');
                $('#EditDepartmentModal').modal('hide');
                _$departmentsTable.ajax.reload();
            },
            error: function (xhr, status, error) {
                // Handle error response
                toastr.error('Failed to update department!');
                console.error('Error updating department:', error);
            }
        });
    });

    $(document).on('click', '.delete-department', function () {
         
        let departmentId = parseInt($(this).data("department-id"), 10);

        // Confirm deletion with the user
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/deleteDepartment', // Service endpoint URL
                    type: 'POST', // HTTP method
                    data: {
                        id: departmentId,
                    },
                    success: function () {
                        // Notify the user of success
                        toastr.success('Department deleted successfully');
                        _$departmentsTable.ajax.reload();
                    },
                    error: function (xhr, status, error) {
                        // Notify the user of an error
                        toastr.error('Failed to delete department');
                    },
                    complete: function () {
                    }
                });
            }
        });
    });

    $('.btn-search').on('click', (e) => {
        _$boookingsTable.ajax.reload();
    });

    $('#filterButtonDepartment').on('click', (e) => {
        _$departmentsTable.ajax.reload();
    });

    $('#clearfilterButtonDepartment').on('click', (e) => {
        $('#startDateDepartment').val('');
        $('#endDateDepartment').val('');
        $('#errorMessageDepartment').text("");
        ApplyFilters();
        _$departmentsTable.ajax.reload();
    });


    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$departmentsTable.ajax.reload();
            return false;
        }
    });

    $('#openCreateModalBtn').on('click', function () {
        $('#CreateDepartmentModal').modal('show');
        // Load the partial view into a modal container
    });

    $('#CreateDepartmentModal').on('shown.bs.modal', function () {
        $('#createDepartment')[0].reset();
    });

    $('#CreateDepartmentModal').on('hidden.bs.modal', function () {
        $('#createDepartment')[0].reset();
    });
    $('.closeBtnDepartment').on('click', function () {
        $('#createDepartment')[0].reset();
        $('#CreateDepartmentModal').modal('hide');
    });

    $('#createDepartment').on('submit', function (event) {
        // Prevent default form submission
        event.preventDefault();

        // Send AJAX request
        let formData = $(this).serialize();
        $.ajax({
            type: 'POST',
            url: '/createDepartment',
            data: formData,
            success: function (response) {
                toastr.success('Department added successfully');
                _$departmentsTable.ajax.reload();
                $('#createDepartment')[0].reset();
                $('#CreateDepartmentModal').modal('hide');
            },
            error: function (xhr, status, error) {
                toastr.error('Failed to add department');
            }
        });
    });
});

function ApplyFilters() {
    // Initialize elements and current date
    let startDateInputDepartment = $('#startDateDepartment');
    let endDateInputDepartment = $('#endDateDepartment');
    let filterButtonDepartment = $('#filterButtonDepartment');
    let clearFilterButtonDepartment = $('#clearfilterButtonDepartment');
    let errorMessageDivForDepartment = $('#errorMessageDepartment');
    let currentDate = new Date();
    currentDate.setHours(0, 0, 0, 0);

    // Disable buttons initially
    filterButtonDepartment.addClass('btn bg-blue').prop('disabled', true);
    clearFilterButtonDepartment.addClass('btn bg-blue').prop('disabled', true);

    function validateDates() {
         
        let startDate = new Date(startDateInputDepartment.val());
        startDate.setHours(0, 0, 0, 0);
        let endDate = new Date(endDateInputDepartment.val());
        endDate.setHours(0, 0, 0, 0);

        // Initialize error message
        let errorMessage = '';

        // Validate start date
        if (startDateInputDepartment.val() && startDate > currentDate) {
            errorMessage = "Start date cannot be in the future.";
        }
        // Validate end date
        else if (endDateInputDepartment.val() && endDate > currentDate) {
            errorMessage = "End date cannot be in the future.";
        }
        // Validate start date is not after end date
        else if (startDateInputDepartment.val() && endDateInputDepartment.val() && startDate > endDate) {
            errorMessage = "Start date cannot be after end date.";
        }

        // Display error message or enable buttons
        if (errorMessage) {
            filterButtonDepartment.prop('disabled', true);
            errorMessageDivForDepartment.text(errorMessage).css('color', 'red');
        } else {
            errorMessageDivForDepartment.text("");
            filterButtonDepartment.prop('disabled', false);
            clearFilterButtonDepartment.prop('disabled', false);
        }
    }

    // Attach input event listeners
    startDateInputDepartment.on('input', validateDates);
    endDateInputDepartment.on('input', validateDates);
}
