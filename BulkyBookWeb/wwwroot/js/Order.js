var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#productsTable').DataTable({
        'ajax': {
            "url": "/Admin/Order/GetAllOrders"
        },
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                         <a href="/Admin/Product/Details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Details</a>
                          </div>
                    `
                }
            }

        ]
    })
}

