import { BacklogItemGetResponseBug } from './BacklogItemGetResponseBug';
import { BacklogItemGetResponseFeature } from './BacklogItemGetResponseFeature';
import { BacklogItemGetResponseTask } from './BacklogItemGetResponseTask';
import { BacklogItemGetResponseUserStory } from './BacklogItemGetResponseUserStory';

export type BacklogItemGetResponseAllFields = BacklogItemGetResponseBug &
	BacklogItemGetResponseUserStory &
	BacklogItemGetResponseTask &
	BacklogItemGetResponseFeature;
