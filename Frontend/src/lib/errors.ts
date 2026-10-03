import toast from 'svelte-french-toast';

type ValidationProblem = {
	title?: string;
	detail?: string;
	errors?: Record<string, string[]>;
};

const FieldLabels: Record<string, string> = {
	displayName: 'Display name',
	biography: 'Biography',
	name: 'Name',
	description: 'Description',
	parentGroupId: 'Parent group',
	skip: 'Skip',
	limit: 'Limit'
};

function isRecord(value: unknown): value is Record<string, unknown> {
	return typeof value === 'object' && value !== null;
}

function toProblem(error: unknown): ValidationProblem | null {
	if (typeof error === 'string') {
		try {
			return toProblem(JSON.parse(error));
		} catch {
			return null;
		}
	}

	if (!isRecord(error)) return null;

	const errors = error.errors;
	const fieldErrors: Record<string, string[]> = {};

	if (isRecord(errors)) {
		for (const [field, messages] of Object.entries(errors)) {
			if (Array.isArray(messages)) fieldErrors[field] = messages.map(String);
		}
	}

	return {
		title: typeof error.title === 'string' ? error.title : undefined,
		detail: typeof error.detail === 'string' ? error.detail : undefined,
		errors: Object.keys(fieldErrors).length > 0 ? fieldErrors : undefined
	};
}

export function formatFieldName(field: string): string {
	const key = field.charAt(0).toLowerCase() + field.slice(1);
	return FieldLabels[key] ?? key.replace(/([A-Z])/g, ' $1').replace(/^./, (c) => c.toUpperCase());
}

export function toValidationMessages(error: unknown): string[] {
	const problem = toProblem(error);

	if (!problem) {
		if (error instanceof Error && error.message) return [error.message];
		return typeof error === 'string' && error ? [error] : [];
	}

	if (problem.errors) {
		return Object.entries(problem.errors).flatMap(([field, messages]) =>
			messages.map((message) => `${formatFieldName(field)}: ${message}`)
		);
	}

	const message = problem.detail ?? problem.title;
	return message ? [message] : [];
}

export function showRequestError(error: unknown): string[] {
	const messages = toValidationMessages(error);

	if (messages.length === 0) toast.error('Something went wrong. Please try again.');
	else for (const message of messages) toast.error(message);

	return messages;
}
