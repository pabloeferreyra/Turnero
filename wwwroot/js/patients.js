(function () {
    'use strict';

    window.Patients = window.Patients || {};
    const P = Patients;

    P.State = {
        currentPage: 1,
        pageSize: parseInt($('#pageSize').val() || -1),
        recordsTotal: 0,
        currentData: []
    };

    P.Pagination = {
        goTo(page) {
            P.State.currentPage = page;
            Patients.Data.load();
        },
        next() {
            if (P.State.currentPage * P.State.pageSize < P.State.recordsTotal) {
                P.State.currentPage++;
                Patients.Data.load();
            }
        },
        prev() {
            if (P.State.currentPage > 1) {
                P.State.currentPage--;
                Patients.Data.load();
            }
        },
        render() {
            const pagination = $('#patients-pagination');
            pagination.empty();

            if (P.State.pageSize === -1 || P.State.pageSize === 0) return;

            const totalPages = Math.ceil(P.State.recordsTotal / P.State.pageSize) || 1;
            const startPage = Math.max(1, P.State.currentPage - 2);
            const endPage = Math.min(totalPages, P.State.currentPage + 2);

            if (P.State.currentPage > 1) {
                pagination.append(`<li class="page-item"><a class="page-link" href="#" onclick="Patients.Pagination.prev();return false;">Anterior</a></li>`);
            }

            for (let p = startPage; p <= endPage; p++) {
                const active = p === P.State.currentPage ? ' active' : '';
                pagination.append(`<li class="page-item${active}"><a class="page-link" href="#" onclick="Patients.Pagination.goTo(${p});return false;">${p}</a></li>`);
            }

            if (P.State.currentPage < totalPages) {
                pagination.append(`<li class="page-item"><a class="page-link" href="#" onclick="Patients.Pagination.next();return false;">Siguiente</a></li>`);
            }
        }
    };

    P.Data = {
        load() {
            var search = $('#searchBox').val() || '';
            var draw = 1;
            var start = (P.State.currentPage - 1) * (P.State.pageSize === -1 ? 0 : P.State.pageSize);
            var length = P.State.pageSize === -1 ? -1 : P.State.pageSize;

            var columns = {
                'columns[0][data]': 'Name',
                'columns[1][data]': 'DNI',
                'columns[2][data]': 'BirthDate',
                'columns[3][data]': 'SocialWork',
                'columns[4][data]': 'AffiliateNumber',
                'columns[5][data]': 'Actions'
            };

            var dataToSend = {
                draw,
                start,
                length,
                'order[0][column]': 5,
                'order[0][dir]': 'desc',
                search
            };

            for (var k in columns) dataToSend[k] = columns[k];

            dataToSend['columns[1][search][value]'] = search || '';
            dataToSend['columns[2][search][value]'] = search || '';

            $.ajax({
                type: "POST",
                url: "/Patients/InitializePatients",
                data: dataToSend,
                success: function (response) {
                    var data = response && (response.data || response.Data || response.items || response.Items);

                    P.State.currentData = data || [];
                    P.State.recordsTotal = response && (response.recordsTotal || response.recordsFiltered || P.State.currentData.length) || 0;

                    if (length === -1) P.State.pageSize = P.State.recordsTotal;

                    P.Data.renderTable();
                },
                error: function () {
                    Swal.fire({ icon: 'error', title: 'Error cargando pacientes', timer: 1200 });
                }
            });
        },

        renderTable() {
            var $tbody = $('#patients-body');
            $tbody.empty();

            if (!P.State.currentData || P.State.currentData.length === 0) {
                $tbody.append('<tr><td colspan="6" class="text-center">No se encontraron pacientes.</td></tr>');
                $('#pageInfo').text('Mostrando 0 de 0 pacientes');
                return;
            }

            P.State.currentData.forEach(function (patient) {
                var birthdate = (patient.birthDate ? (patient.birthDate.split(' ')[0] || '') : '');
                var row = `<tr>
                    <td>${patient.name || ''}</td>
                    <td>${patient.dni || ''}</td>
                    <td>${birthdate}</td>
                    <td>${patient.socialWork || ''}</td>
                    <td>${patient.affiliateNumber || ''}</td>
                    <td>
                        <div class="btn-group" role="group">
                            <button class="btn btn-sm btn-primary me-1 btn-edit-patient" data-id="${patient.id}">Editar</button>
                            <a href="/Patients/Details/${patient.id}" class="btn btn-sm btn-secondary me-1">Detalles</a>
                            <button class="btn btn-sm btn-danger" onclick="confirmDelete(${patient.id})">Eliminar</button>
                        </div>
                    </td>
                </tr>`;
                $tbody.append(row);
            });

            Patients.Pagination.render();
        }
    };

    P.UI = {
        openCreate() {
            $.ajax({
                type: 'GET',
                url: '/Patients/Create',
                success: function (html) {
                    $("#CreateFormContent").html(html);
                    $('#Create').modal('show');

                    document.dispatchEvent(new Event("patients:createLoaded"));
                }
            });
        },
        openEdit(id) {
            $.ajax({
                type: 'GET',
                url: '/Patients/Edit',
                data: { id },
                success: function (html) {
                    $("#EditFormContent").html(html);
                    $('#Edit').modal('show');

                    document.dispatchEvent(new Event("patients:editLoaded"));
                }
            });
        }
    };

    P.Events = {
        wire() {
            $(document).on('click', '#createPatient', function () {
                P.UI.openCreate();
            });

            $(document).on('click', '#patients tbody .btn-edit-patient', function () {
                const id = $(this).data('id');
                P.UI.openEdit(id);
            });

            $(document).on('click', '#btnSearch', function () {
                P.State.currentPage = 1;
                P.Data.load();
            });

            $(document).on('change', '#pageSize', function () {
                P.State.pageSize = parseInt($(this).val()) || -1;
                P.State.currentPage = 1;
                P.Data.load();
            });

            $(document).on('input', '#searchBox', function () {
                P.State.currentPage = 1;
                P.Data.load();
            });
        }
    };

})();

$(document).on('click', '#myTabs .nav-link', function () {
    $('#myTabs .nav-link').removeClass('active');
    $(this).addClass('active');
});

$(document).ready(function () {
    Patients.Events.wire();
    AppUtils.Sort.attachHeaderSorting('#patients');
    Patients.Data.load();
});