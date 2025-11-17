// @ts-nocheck
(function () {

    const namespace = 'parentsData';
    const tabSelector = '#parentsTab';

    $(document).ready(function () {

        const $tab = $(tabSelector);
        if (!$tab.length) return;

        $tab.off(`click.${namespace}`).on(`click.${namespace}`, function () {
            loadParentsData();
        });

        $('#tabContent')
            .off(`click.${namespace}`, '[data-edit-parent]')
            .on(`click.${namespace}`, '[data-edit-parent]', function () {
                const id = $(this).data('id');
                editParent(id);
            });
    });

    function resolvePatientId() {
        return $('#patientId').val() || '';
    }

    function loadParentsData() {
        const patientId = resolvePatientId();
        if (!patientId) return;

        const url = `/ParentsData/Index?id=${encodeURIComponent(patientId)}`;

        $('#myTabs .nav-link').removeClass('active');
        $('#parentsTab').addClass('active');

        $('#tabContent').load(url, function () {
            document.dispatchEvent(new Event('parents:dataLoaded'));
        });
    }

    function editParent(id) {
        const url = `/ParentsData/Edit?id=${encodeURIComponent(id)}`;

        $('#tabContent').load(url, function () {
            document.dispatchEvent(new Event('parents:editLoaded'));
        });
    }

    window.parents_loadData = loadParentsData;
})();
