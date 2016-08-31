import { loadPageDef } from 'console/state/actions/pageDefs.js';
import { loadValues } from 'console/state/actions/values.js';
import { selectShownPage, replacePages } from 'console/state/reducers/pages.js';

export default function initState(store) {
	store.dispatch(loadPageDef('edit-language'));
	store.dispatch(loadValues('edit-language'));
	store.dispatch(replacePages(['edit-language', 'other-page']));
	store.dispatch(selectShownPage('edit-language'));
}
